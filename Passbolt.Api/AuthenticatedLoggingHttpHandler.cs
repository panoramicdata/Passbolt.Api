using Passbolt.Api.Mfa;

namespace Passbolt.Api;

internal sealed partial class AuthenticatedLoggingHttpHandler : HttpClientHandler
{
	private readonly PassboltClientOptions _options;
	private readonly ILogger _logger;
	private readonly LogLevel _levelToLogAt = LogLevel.Debug;
	private readonly SemaphoreSlim _authenticateSemaphore = new(1, 1);
	private readonly CookieContainer _cookieContainer = new();
	private readonly string _privateKeyFingerprint;
	private volatile bool _isAuthenticated;
	private string? _csrfToken;

	private const string LoginPath = "/auth/login.json";
	private const string UsersMePath = "/users/me.json";
	private const string SessionCookieName = "passbolt_session";
	private const string MfaCookieName = "passbolt_mfa";
	private const string MfaVerifyTotpPath = "/mfa/verify/totp.json";
	private const string ShareSimulatePathPrefix = "/share/simulate/";

	private sealed class LoginContainer
	{
		[JsonPropertyName("gpg_auth")]
		public required LoginRequest GpgAuth { get; init; }
	}

	private sealed class LoginRequest
	{
		[JsonPropertyName("keyid")]
		public required string KeyId { get; init; }

		[JsonPropertyName("user_token_result")]
		public string? Token { get; init; }
	}

	private sealed class EnvelopeHeader
	{
		[JsonPropertyName("status")]
		public string? Status { get; init; }
	}

	private sealed class Envelope
	{
		[JsonPropertyName("header")]
		public EnvelopeHeader? Header { get; init; }
	}

	private static readonly JsonSerializerOptions JsonSerializerOptions = new()
	{
		PropertyNameCaseInsensitive = true,
		DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
		// .NET 10 disables reflection-based serialization by default; an explicit resolver keeps the
		// client working regardless of the host application's reflection setting.
		TypeInfoResolver = new DefaultJsonTypeInfoResolver(),
	};

	public AuthenticatedLoggingHttpHandler(PassboltClientOptions options)
	{
		_options = options;
		_logger = options.Logger;
		_privateKeyFingerprint = GetPrivateKeyFingerprint(options.PrivateKeyBlock);

		UseCookies = true;
		CookieContainer = _cookieContainer;
	}

	protected override async Task<HttpResponseMessage> SendAsync(
		HttpRequestMessage request,
		CancellationToken cancellationToken)
	{
		if (_options.IsReadOnly && ChangesServer(request))
		{
			throw new InvalidOperationException($"The client is read-only; {request.Method} {request.RequestUri?.AbsolutePath} would change the server.");
		}

		var logPrefix = $"Request {Guid.NewGuid()}: ";

		await EnsureAuthenticatedAsync(cancellationToken).ConfigureAwait(false);
		ApplyAuthenticationHeaders(request);

		if (_logger.IsEnabled(_levelToLogAt))
		{
			await LogRequestAsync(logPrefix, request, cancellationToken).ConfigureAwait(false);
		}

		var response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);

		if (_logger.IsEnabled(_levelToLogAt))
		{
			await LogResponseAsync(logPrefix, response, cancellationToken).ConfigureAwait(false);
		}

		return response;
	}

	// A share simulation is a POST that only reports what a share would do, so it is allowed by path.
	private static bool ChangesServer(HttpRequestMessage request)
		=> request.Method != HttpMethod.Get
			&& request.Method != HttpMethod.Head
			&& request.Method != HttpMethod.Options
			&& request.RequestUri?.AbsolutePath.StartsWith(ShareSimulatePathPrefix, StringComparison.OrdinalIgnoreCase) != true;

	private async Task EnsureAuthenticatedAsync(CancellationToken cancellationToken)
	{
		if (_isAuthenticated)
		{
			return;
		}

		await _authenticateSemaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
		try
		{
			if (_isAuthenticated)
			{
				return;
			}

			await RestoreRememberedMfaCookieAsync(cancellationToken).ConfigureAwait(false);

			var encryptedAuthToken = await LoginStage1Async(cancellationToken).ConfigureAwait(false);
			var userAuthToken = DecryptAuthToken(encryptedAuthToken, _options.PrivateKeyBlock, _options.Password);
			ValidateAuthTokenFormat(userAuthToken);

			var stage2 = await LoginStage2Async(userAuthToken, cancellationToken).ConfigureAwait(false);
			if (!IsSuccessEnvelope(stage2))
			{
				throw new InvalidOperationException("Passbolt authentication failed during stage-2 token exchange.");
			}

			var baseUri = new Uri(_options.Uri, LoginPath);
			EnsureSessionCookieExists(baseUri);

			if (GetCookie(baseUri, "csrfToken") is null || _options.Mfa is not null)
			{
				await EnsureSecondFactorAsync(cancellationToken).ConfigureAwait(false);
			}

			_csrfToken = GetCookie(baseUri, "csrfToken")?.Value;
			_isAuthenticated = true;
		}
		finally
		{
			_authenticateSemaphore.Release();
		}
	}

	private async Task<string> LoginStage1Async(CancellationToken cancellationToken)
	{
		var response = await SendLoginRequestAsync(
			new LoginContainer
			{
				GpgAuth = new LoginRequest
				{
					KeyId = _privateKeyFingerprint
				}
			},
			cancellationToken).ConfigureAwait(false);

		using (response)
		{
			return GetEncryptedAuthToken(response);
		}
	}

	private async Task<Envelope> LoginStage2Async(string userAuthToken, CancellationToken cancellationToken)
	{
		var response = await SendLoginRequestAsync(
			new LoginContainer
			{
				GpgAuth = new LoginRequest
				{
					KeyId = _privateKeyFingerprint,
					Token = userAuthToken
				}
			},
			cancellationToken).ConfigureAwait(false);

		using (response)
		{
			return await ReadEnvelopeAsync(response, cancellationToken).ConfigureAwait(false);
		}
	}

	private async Task<HttpResponseMessage> SendLoginRequestAsync(
		LoginContainer payload,
		CancellationToken cancellationToken)
	{
		var loginUri = new Uri(_options.Uri, LoginPath);
		var httpRequest = new HttpRequestMessage(HttpMethod.Post, loginUri)
		{
			Content = JsonContent.Create(payload, options: JsonSerializerOptions)
		};

		httpRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

		if (_logger.IsEnabled(_levelToLogAt))
		{
			await LogRequestAsync("Auth: ", httpRequest, cancellationToken).ConfigureAwait(false);
		}

		var response = await base.SendAsync(httpRequest, cancellationToken).ConfigureAwait(false);

		if (_logger.IsEnabled(_levelToLogAt))
		{
			await LogResponseAsync("Auth: ", response, cancellationToken).ConfigureAwait(false);
		}

		response.EnsureSuccessStatusCode();
		return response;
	}

	private static async Task<Envelope> ReadEnvelopeAsync(HttpResponseMessage response, CancellationToken cancellationToken)
	{
		var body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
		return JsonSerializer.Deserialize<Envelope>(body, JsonSerializerOptions)
			?? throw new InvalidOperationException("Passbolt response did not contain a valid JSON envelope.");
	}

	private void ApplyAuthenticationHeaders(HttpRequestMessage request)
	{
		if (!string.IsNullOrWhiteSpace(_csrfToken))
		{
			request.Headers.Remove("X-CSRF-Token");
			request.Headers.TryAddWithoutValidation("X-CSRF-Token", _csrfToken);
		}
	}
}
