using Org.BouncyCastle.Bcpg;
using Org.BouncyCastle.Bcpg.OpenPgp;
using Org.BouncyCastle.Security;

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
		// PgpEncryptedDataList reads its ciphertext lazily from the underlying stream, so the input and
		// decoder streams MUST remain open until decryption and literal extraction have completed.
		// Previously the parsing lived in a helper whose `using` blocks disposed those streams on
		// return, so the later GetDataStream call failed with "Cannot access a closed Stream" (surfaced
		// as "Exception starting decryption"). Keep everything in one scope. See issue #32.
		using var encryptedInput = new MemoryStream(Encoding.UTF8.GetBytes(armoredMessage));
		using var decoderStream = PgpUtilities.GetDecoderStream(encryptedInput);

		var factory = new PgpObjectFactory(decoderStream);
		var encryptedDataList = factory.NextPgpObject() as PgpEncryptedDataList
			?? factory.NextPgpObject() as PgpEncryptedDataList
			?? throw new InvalidOperationException("Failed to parse encrypted Passbolt auth token.");

		var bundle = ReadSecretKeyBundle(privateKeyArmored);
		using var clearStream = GetDecryptedStream(encryptedDataList, bundle, password);
		return ExtractLiteralData(clearStream);
	}

	private static PgpSecretKeyRingBundle ReadSecretKeyBundle(string privateKeyArmored)
	{
		using var keyStream = new MemoryStream(Encoding.UTF8.GetBytes(privateKeyArmored));
		using var keyDecoderStream = PgpUtilities.GetDecoderStream(keyStream);
		return new PgpSecretKeyRingBundle(keyDecoderStream);
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

	private static string ExtractLiteralData(Stream clearStream)
	{
		var plainFactory = new PgpObjectFactory(clearStream);
		var plainObject = plainFactory.NextPgpObject();

		// If the message is compressed, the literal data packet is read lazily from the
		// decompression stream, so it must stay open until the literal has been fully read
		// (mirrors the ciphertext-stream lifetime fix for issue #32).
		Stream? decompressedStream = null;
		try
		{
			if (plainObject is PgpCompressedData compressedData)
			{
				decompressedStream = compressedData.GetDataStream();
				plainFactory = new PgpObjectFactory(decompressedStream);
				plainObject = plainFactory.NextPgpObject();
			}

			var literalData = AdvanceToLiteralData(plainFactory, plainObject);

			using var literalStream = literalData.GetInputStream();
			using var reader = new StreamReader(literalStream, Encoding.UTF8);
			return reader.ReadToEnd();
		}
		finally
		{
			decompressedStream?.Dispose();
		}
	}

	/// <summary>
	/// Walks the PGP object stream to the literal data packet, skipping the signature and marker
	/// packets that legitimately precede it and rejecting anything else.
	/// </summary>
	private static PgpLiteralData AdvanceToLiteralData(PgpObjectFactory factory, PgpObject? pgpObject)
	{
		while (pgpObject is not null and not PgpLiteralData)
		{
			if (!IsSkippablePacket(pgpObject))
			{
				throw new InvalidOperationException($"Passbolt auth token contained unsupported PGP payload type {pgpObject.GetType().Name}.");
			}

			pgpObject = factory.NextPgpObject();
		}

		return pgpObject as PgpLiteralData
			?? throw new InvalidOperationException("Passbolt auth token did not contain a literal data packet.");
	}

	private static bool IsSkippablePacket(PgpObject pgpObject)
		=> pgpObject is PgpOnePassSignatureList
			or PgpSignatureList
			or PgpMarker;

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
