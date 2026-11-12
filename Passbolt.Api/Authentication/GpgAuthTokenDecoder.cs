using Org.BouncyCastle.Bcpg.OpenPgp;
using Passbolt.Api.Cryptography;

namespace Passbolt.Api.Authentication;

/// <summary>
/// Decrypts the GPGAuth challenge token Passbolt returns at login: an ASCII-armored OpenPGP
/// message carrying a plaintext auth token, decrypted with the user's private key.
/// </summary>
internal static class GpgAuthTokenDecoder
{
	/// <summary>
	/// Decrypts an ASCII-armored OpenPGP message using the supplied private key.
	/// </summary>
	/// <param name="armoredMessage">The ASCII-armored OpenPGP message.</param>
	/// <param name="privateKeyArmored">The ASCII-armored private key that can decrypt the message.</param>
	/// <param name="password">The passphrase protecting the private key (empty string if unprotected).</param>
	/// <returns>The decrypted plaintext.</returns>
	public static string Decrypt(string armoredMessage, string privateKeyArmored, string password)
	{
		// The ciphertext is read lazily, so these streams must stay open until extraction finishes (#32).
		using var encryptedInput = new MemoryStream(Encoding.UTF8.GetBytes(armoredMessage));
		using var decoderStream = PgpUtilities.GetDecoderStream(encryptedInput);

		var encryptedDataList = PassboltPgp.ReadEncryptedDataList(decoderStream, "Failed to parse encrypted Passbolt auth token.");

		var bundle = PassboltPgp.ReadSecretKeyBundle(privateKeyArmored);
		using var clearStream = GetDecryptedStream(encryptedDataList, bundle, password);
		return PassboltPgp.ExtractLiteralData(clearStream, "Passbolt auth token", "Passbolt auth token contained unsupported PGP payload type");
	}

	private static Stream GetDecryptedStream(PgpEncryptedDataList encryptedDataList, PgpSecretKeyRingBundle bundle, string password)
	{
		var publicKeyEncryptedDataPackets = encryptedDataList
			.GetEncryptedDataObjects()
			.OfType<PgpPublicKeyEncryptedData>()
			.ToList();

		if (publicKeyEncryptedDataPackets.Count == 0)
		{
			throw new InvalidOperationException("No encrypted data payload found in Passbolt auth token.");
		}

		return TryGetDecryptedDataStream(publicKeyEncryptedDataPackets, bundle, password)
			?? throw new InvalidOperationException("Unable to decrypt Passbolt auth token with the configured private key.");
	}

	private static Stream? TryGetDecryptedDataStream(
		IEnumerable<PgpPublicKeyEncryptedData> encryptedPackets,
		PgpSecretKeyRingBundle bundle,
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
			catch (PgpException)
			{
				// Try next key packet; message may target a different recipient key.
			}
		}

		return null;
	}

	private static PgpPrivateKey? GetPrivateKey(
		PgpSecretKeyRingBundle bundle,
		long keyId,
		string password)
	{
		var secretKey = bundle.GetSecretKey(keyId);
		if (secretKey is not null)
		{
			return secretKey.ExtractPrivateKey(password.ToCharArray());
		}

		foreach (PgpSecretKeyRing ring in bundle.GetKeyRings())
		{
			foreach (PgpSecretKey candidate in ring.GetSecretKeys())
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
}
