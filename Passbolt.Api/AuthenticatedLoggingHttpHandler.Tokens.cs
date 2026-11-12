
namespace Passbolt.Api;

/// <summary>Auth-token, cookie and key helpers for the sign-in.</summary>
internal sealed partial class AuthenticatedLoggingHttpHandler
{
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
}
