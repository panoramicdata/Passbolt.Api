using Passbolt.Api.Mfa;

namespace Passbolt.Api;

internal sealed class AuthenticatedLoggingHttpHandler : HttpClientHandler
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

	// A signed-in request to /users/me.json seeds the CSRF cookie and shows whether the server wants a second factor.
	private async Task<bool> ProbeForSecondFactorAsync(CancellationToken cancellationToken)
	{
		var meUri = new Uri(_options.Uri, UsersMePath);
		using var request = new HttpRequestMessage(HttpMethod.Get, meUri);
		request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

		using var response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
		if (response.StatusCode == HttpStatusCode.Forbidden
			&& PassboltMfaChallenge.OffersTotp(await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false)))
		{
			return true;
		}

		response.EnsureSuccessStatusCode();
		return false;
	}

	private async Task EnsureSecondFactorAsync(CancellationToken cancellationToken)
	{
		if (!await ProbeForSecondFactorAsync(cancellationToken).ConfigureAwait(false))
		{
			return;
		}

		var mfa = _options.Mfa;
		if (mfa?.CookieStore is { } store)
		{
			await store.ClearAsync(cancellationToken).ConfigureAwait(false);
		}

		var code = mfa?.TotpProvider is { } provider
			? await provider(cancellationToken).ConfigureAwait(false)
			: null;
		if (string.IsNullOrWhiteSpace(code))
		{
			throw new PassboltMfaRequiredException("Passbolt requires a one-time code for this device and none was supplied. Sign in once interactively with a TotpProvider and RememberDevice set to remember this device.");
		}

		await VerifyTotpAsync(code.Trim(), cancellationToken).ConfigureAwait(false);

		if (await ProbeForSecondFactorAsync(cancellationToken).ConfigureAwait(false))
		{
			throw new PassboltMfaRequiredException("Passbolt still requires a second factor after the one-time code was accepted.");
		}
	}

	private async Task VerifyTotpAsync(string code, CancellationToken cancellationToken)
	{
		var remember = _options.Mfa?.RememberDevice ?? true;

		using var request = BuildTotpVerifyRequest(code, remember);
		using var response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
		if (!response.IsSuccessStatusCode)
		{
			throw new PassboltMfaRequiredException($"Passbolt refused the one-time code (HTTP {(int)response.StatusCode}).");
		}

		if (remember)
		{
			await RememberDeviceAsync(cancellationToken).ConfigureAwait(false);
		}
	}

	private HttpRequestMessage BuildTotpVerifyRequest(string code, bool remember)
	{
		var request = new HttpRequestMessage(HttpMethod.Post, new Uri(_options.Uri, MfaVerifyTotpPath))
		{
			Content = JsonContent.Create(new MfaVerifyRequest { Totp = code, Remember = remember ? 1 : 0 }, options: JsonSerializerOptions)
		};
		request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
		if (GetCookie(new Uri(_options.Uri, LoginPath), "csrfToken")?.Value is { Length: > 0 } csrfToken)
		{
			request.Headers.TryAddWithoutValidation("X-CSRF-Token", csrfToken);
		}

		return request;
	}

	private async Task RememberDeviceAsync(CancellationToken cancellationToken)
	{
		if (_options.Mfa?.CookieStore is { } store
			&& GetCookie(new Uri(_options.Uri, LoginPath), MfaCookieName)?.Value is { Length: > 0 } rememberedValue)
		{
			await store.SaveAsync(rememberedValue, cancellationToken).ConfigureAwait(false);
		}
	}
	private async Task RestoreRememberedMfaCookieAsync(CancellationToken cancellationToken)
	{
		if (_options.Mfa?.CookieStore is not { } store
			|| await store.LoadAsync(cancellationToken).ConfigureAwait(false) is not { Length: > 0 } value)
		{
			return;
		}

		_cookieContainer.Add(_options.Uri, new Cookie(MfaCookieName, value, "/", _options.Uri.Host)
		{
			Secure = _options.Uri.Scheme == Uri.UriSchemeHttps
		});
	}

	private sealed class MfaVerifyRequest
	{
		[JsonPropertyName("totp")]
		public required string Totp { get; init; }

		[JsonPropertyName("remember")]
		public int Remember { get; init; }
	}
	private void ApplyAuthenticationHeaders(HttpRequestMessage request)
	{
		if (!string.IsNullOrWhiteSpace(_csrfToken))
		{
			request.Headers.Remove("X-CSRF-Token");
			request.Headers.TryAddWithoutValidation("X-CSRF-Token", _csrfToken);
		}
	}

	private static string GetEncryptedAuthToken(HttpResponseMessage response)
	{
		if (!response.Headers.TryGetValues("X-GPGAuth-User-Auth-Token", out var values))
		{
			throw new InvalidOperationException("Passbolt login stage-1 response is missing X-GPGAuth-User-Auth-Token header.");
		}

		var token = values.FirstOrDefault();
		if (string.IsNullOrWhiteSpace(token))
		{
			throw new InvalidOperationException("Passbolt login stage-1 response returned an empty auth-token header.");
		}

		var decodedToken = System.Net.WebUtility.UrlDecode(token)
			?? throw new InvalidOperationException("Passbolt login stage-1 response auth-token could not be URL decoded.");

		return decodedToken.Replace("\\ ", " ", StringComparison.Ordinal);
	}

	private void EnsureSessionCookieExists(Uri uri)
	{
		var cookie = GetCookie(uri, SessionCookieName)
			?? GetCookie(uri, "CAKEPHP")
			?? GetCookie(uri, "PHPSESSID");

		if (cookie is null)
		{
			throw new InvalidOperationException("Passbolt login stage-2 response did not return a session cookie.");
		}
	}

	private Cookie? GetCookie(Uri uri, string name)
	{
		foreach (Cookie cookie in _cookieContainer.GetCookies(uri))
		{
			if (string.Equals(cookie.Name, name, StringComparison.Ordinal))
			{
				return cookie;
			}
		}

		return null;
	}

	private static bool IsSuccessEnvelope(Envelope envelope)
		=> string.Equals(envelope.Header?.Status, "success", StringComparison.OrdinalIgnoreCase);

	private static void ValidateAuthTokenFormat(string token)
	{
		var fields = token.Split('|');
		if (fields.Length != 4)
		{
			throw new InvalidOperationException("Passbolt auth token format is invalid (expected 4 fields).", new FormatException(token));
		}

		if (!string.Equals(fields[0], "gpgauthv1.3.0", StringComparison.Ordinal)
			|| !string.Equals(fields[3], "gpgauthv1.3.0", StringComparison.Ordinal))
		{
			throw new InvalidOperationException("Passbolt auth token version fields are invalid.", new FormatException(token));
		}
	}

	// internal (not private) so the GPGAuth decrypt path has a deterministic unit test (issue #32).
	internal static string DecryptAuthToken(string armoredMessage, string privateKeyArmored, string password)
		=> Authentication.GpgAuthTokenDecoder.Decrypt(armoredMessage, privateKeyArmored, password);

	private static string GetPrivateKeyFingerprint(string privateKeyArmored)
	{
		using var keyStream = new MemoryStream(Encoding.UTF8.GetBytes(privateKeyArmored));
		using var decoderStream = Org.BouncyCastle.Bcpg.OpenPgp.PgpUtilities.GetDecoderStream(keyStream);
		var bundle = new Org.BouncyCastle.Bcpg.OpenPgp.PgpSecretKeyRingBundle(decoderStream);

		foreach (Org.BouncyCastle.Bcpg.OpenPgp.PgpSecretKeyRing ring in bundle.GetKeyRings())
		{
			foreach (Org.BouncyCastle.Bcpg.OpenPgp.PgpSecretKey secretKey in ring.GetSecretKeys())
			{
				if (!secretKey.IsSigningKey)
				{
					continue;
				}

				return Convert.ToHexString(secretKey.PublicKey.GetFingerprint());
			}
		}

		throw new InvalidOperationException("Unable to determine fingerprint from private PGP key.");
	}

	private async Task LogRequestAsync(string logPrefix, HttpRequestMessage request, CancellationToken cancellationToken)
	{
		_logger.Log(_levelToLogAt, "{LogPrefix}{Method} {Uri}", logPrefix, request.Method, request.RequestUri);
		_logger.Log(_levelToLogAt, "{LogPrefix}Request Headers:\r\n{Headers}", logPrefix, FormatHeaders(request.Headers));

		if (request.Content is null)
		{
			return;
		}

		_logger.Log(_levelToLogAt, "{LogPrefix}Request Content Headers:\r\n{Headers}", logPrefix, FormatHeaders(request.Content.Headers));
		var requestContent = await request.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
		_logger.Log(_levelToLogAt, "{LogPrefix}Request Body:\r\n{RequestContent}", logPrefix, requestContent);
	}

	private async Task LogResponseAsync(string logPrefix, HttpResponseMessage response, CancellationToken cancellationToken)
	{
		_logger.Log(_levelToLogAt, "{LogPrefix}Response {StatusCode}", logPrefix, (int)response.StatusCode);
		_logger.Log(_levelToLogAt, "{LogPrefix}Response Headers:\r\n{Headers}", logPrefix, FormatHeaders(response.Headers));

		if (response.Content is null)
		{
			return;
		}

		_logger.Log(_levelToLogAt, "{LogPrefix}Response Content Headers:\r\n{Headers}", logPrefix, FormatHeaders(response.Content.Headers));
		var responseContent = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
		_logger.Log(_levelToLogAt, "{LogPrefix}Response Body:\r\n{ResponseContent}", logPrefix, responseContent);
	}

	private static string FormatHeaders(HttpHeaders headers)
	{
		var builder = new StringBuilder();

		foreach (var header in headers)
		{
			var value = string.Equals(header.Key, "Authorization", StringComparison.OrdinalIgnoreCase)
				? "***MASKED***"
				: string.Join(", ", header.Value);

			builder.AppendLine($"  {header.Key}: {value}");
		}

		return builder.ToString().TrimEnd();
	}
}
