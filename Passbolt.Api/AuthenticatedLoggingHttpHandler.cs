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

			if (GetCookie(baseUri, "csrfToken") is null)
			{
				await SeedCsrfCookieAsync(cancellationToken).ConfigureAwait(false);
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

	private async Task SeedCsrfCookieAsync(CancellationToken cancellationToken)
	{
		var meUri = new Uri(_options.Uri, UsersMePath);
		using var request = new HttpRequestMessage(HttpMethod.Get, meUri);
		request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

		var response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
		using (response)
		{
			response.EnsureSuccessStatusCode();
		}
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

	private static string DecryptAuthToken(string armoredMessage, string privateKeyArmored, string password)
	{
		using var encryptedInput = new MemoryStream(Encoding.UTF8.GetBytes(armoredMessage));
		using var decoderStream = Org.BouncyCastle.Bcpg.OpenPgp.PgpUtilities.GetDecoderStream(encryptedInput);

		var factory = new Org.BouncyCastle.Bcpg.OpenPgp.PgpObjectFactory(decoderStream);
		var first = factory.NextPgpObject();
		var encryptedDataList = first as Org.BouncyCastle.Bcpg.OpenPgp.PgpEncryptedDataList
			?? factory.NextPgpObject() as Org.BouncyCastle.Bcpg.OpenPgp.PgpEncryptedDataList
			?? throw new InvalidOperationException("Failed to parse encrypted Passbolt auth token.");

		var publicKeyEncryptedDataPackets = encryptedDataList
			.GetEncryptedDataObjects()
			.OfType<Org.BouncyCastle.Bcpg.OpenPgp.PgpPublicKeyEncryptedData>()
			.ToList();

		if (publicKeyEncryptedDataPackets.Count == 0)
		{
			throw new InvalidOperationException("No encrypted data payload found in Passbolt auth token.");
		}

		using var keyStream = new MemoryStream(Encoding.UTF8.GetBytes(privateKeyArmored));
		using var keyDecoderStream = Org.BouncyCastle.Bcpg.OpenPgp.PgpUtilities.GetDecoderStream(keyStream);
		var bundle = new Org.BouncyCastle.Bcpg.OpenPgp.PgpSecretKeyRingBundle(keyDecoderStream);

		var clearStream = TryGetDecryptedDataStream(publicKeyEncryptedDataPackets, bundle, password)
			?? throw new InvalidOperationException("Unable to decrypt Passbolt auth token with the configured private key.");

		using (clearStream)
		{
		var plainFactory = new Org.BouncyCastle.Bcpg.OpenPgp.PgpObjectFactory(clearStream);
		var plainObject = plainFactory.NextPgpObject();

		if (plainObject is Org.BouncyCastle.Bcpg.OpenPgp.PgpCompressedData compressedData)
		{
			using var compressedStream = compressedData.GetDataStream();
			plainFactory = new Org.BouncyCastle.Bcpg.OpenPgp.PgpObjectFactory(compressedStream);
			plainObject = plainFactory.NextPgpObject();
		}

		while (plainObject is not null
			&& plainObject is not Org.BouncyCastle.Bcpg.OpenPgp.PgpLiteralData)
		{
			if (plainObject is Org.BouncyCastle.Bcpg.OpenPgp.PgpOnePassSignatureList
				|| plainObject is Org.BouncyCastle.Bcpg.OpenPgp.PgpSignatureList
				|| plainObject is Org.BouncyCastle.Bcpg.OpenPgp.PgpMarker)
			{
				plainObject = plainFactory.NextPgpObject();
				continue;
			}

			throw new InvalidOperationException($"Passbolt auth token contained unsupported PGP payload type {plainObject.GetType().Name}.");
		}

		if (plainObject is not Org.BouncyCastle.Bcpg.OpenPgp.PgpLiteralData literalData)
		{
			throw new InvalidOperationException("Passbolt auth token did not contain a literal data packet.");
		}

		using var literalStream = literalData.GetInputStream();
		using var reader = new StreamReader(literalStream, Encoding.UTF8);
		return reader.ReadToEnd();
		}
	}

	private static Stream? TryGetDecryptedDataStream(
		IEnumerable<Org.BouncyCastle.Bcpg.OpenPgp.PgpPublicKeyEncryptedData> encryptedPackets,
		Org.BouncyCastle.Bcpg.OpenPgp.PgpSecretKeyRingBundle bundle,
		string password)
	{
		foreach (var packet in encryptedPackets)
		{
			var key = GetPrivateKey(bundle, packet.KeyId, password);
			if (key is null)
			{
				continue;
			}

			try
			{
				return packet.GetDataStream(key);
			}
			catch (Org.BouncyCastle.Bcpg.OpenPgp.PgpException)
			{
				// Try next key packet; message may target a different recipient key.
			}
		}

		return null;
	}

	private static Org.BouncyCastle.Bcpg.OpenPgp.PgpPrivateKey? GetPrivateKey(
		Org.BouncyCastle.Bcpg.OpenPgp.PgpSecretKeyRingBundle bundle,
		long keyId,
		string password)
	{
		var secretKey = bundle.GetSecretKey(keyId);
		if (secretKey is not null)
		{
			return secretKey.ExtractPrivateKey(password.ToCharArray());
		}

		foreach (Org.BouncyCastle.Bcpg.OpenPgp.PgpSecretKeyRing ring in bundle.GetKeyRings())
		{
			foreach (Org.BouncyCastle.Bcpg.OpenPgp.PgpSecretKey candidate in ring.GetSecretKeys())
			{
				if (candidate.KeyId != keyId)
				{
					continue;
				}

				var privateKey = candidate.ExtractPrivateKey(password.ToCharArray());
				if (privateKey is not null)
				{
					return privateKey;
				}
			}
		}

		return null;
	}

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
