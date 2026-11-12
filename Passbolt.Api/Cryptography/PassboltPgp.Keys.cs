using Org.BouncyCastle.Bcpg;
using Org.BouncyCastle.Bcpg.OpenPgp;

namespace Passbolt.Api.Cryptography;

public static partial class PassboltPgp
{
	/// <summary>
	/// Extracts the ASCII-armored public key from an ASCII-armored private key. Used to encrypt a
	/// secret to the current user when creating a personal resource.
	/// </summary>
	/// <param name="privateKeyArmored">The ASCII-armored private key.</param>
	/// <returns>The ASCII-armored public key.</returns>
	public static string ExtractPublicKey(string privateKeyArmored)
	{
		ArgumentNullException.ThrowIfNull(privateKeyArmored);

		var bundle = ReadSecretKeyBundle(privateKeyArmored);
		foreach (PgpSecretKeyRing ring in bundle.GetKeyRings())
		{
			using var armoredOutput = new MemoryStream();
			using (var armorStream = new ArmoredOutputStream(armoredOutput))
			{
				ring.GetPublicKey().PublicKeyPacket.Encode(new BcpgOutputStream(armorStream));
			}

			return Encoding.ASCII.GetString(armoredOutput.ToArray());
		}

		throw new InvalidOperationException("The private key did not contain a key ring.");
	}

	internal static PgpSecretKeyRingBundle ReadSecretKeyBundle(string privateKeyArmored)
	{
		using var keyStream = new MemoryStream(Encoding.UTF8.GetBytes(privateKeyArmored));
		using var decoderStream = PgpUtilities.GetDecoderStream(keyStream);
		return new PgpSecretKeyRingBundle(decoderStream);
	}

	// The caller owns decoderStream: the list reads its ciphertext lazily, so it must outlive decryption.
	internal static PgpEncryptedDataList ReadEncryptedDataList(Stream decoderStream, string failureMessage)
	{
		var factory = new PgpObjectFactory(decoderStream);
		return factory.NextPgpObject() as PgpEncryptedDataList
			?? factory.NextPgpObject() as PgpEncryptedDataList
			?? throw new InvalidOperationException(failureMessage);
	}

	internal static string ExtractLiteralData(Stream clearStream, string subject, string unsupportedPayloadPrefix)
	{
		var factory = new PgpObjectFactory(clearStream);
		var pgpObject = factory.NextPgpObject();

		// A compressed message reads its literal packet lazily from the decompression stream, so
		// that stream must remain open until the literal has been fully read.
		Stream? decompressedStream = null;
		try
		{
			if (pgpObject is PgpCompressedData compressedData)
			{
				decompressedStream = compressedData.GetDataStream();
				factory = new PgpObjectFactory(decompressedStream);
				pgpObject = factory.NextPgpObject();
			}

			var literalData = AdvanceToLiteralData(factory, pgpObject, subject, unsupportedPayloadPrefix);

			using var literalStream = literalData.GetInputStream();
			using var reader = new StreamReader(literalStream, Encoding.UTF8);
			return reader.ReadToEnd();
		}
		finally
		{
			decompressedStream?.Dispose();
		}
	}

	private static PgpLiteralData AdvanceToLiteralData(PgpObjectFactory factory, PgpObject? pgpObject, string subject, string unsupportedPayloadPrefix)
	{
		while (pgpObject is not null and not PgpLiteralData)
		{
			if (pgpObject is not (PgpOnePassSignatureList or PgpSignatureList or PgpMarker))
			{
				throw new InvalidOperationException($"{unsupportedPayloadPrefix} {pgpObject.GetType().Name}.");
			}

			pgpObject = factory.NextPgpObject();
		}

		return pgpObject as PgpLiteralData
			?? throw new InvalidOperationException($"{subject} did not contain a literal data packet.");
	}

	private static PgpSecretKey FindSigningSecretKey(PgpSecretKeyRingBundle bundle)
	{
		foreach (PgpSecretKeyRing ring in bundle.GetKeyRings())
		{
			foreach (PgpSecretKey secretKey in ring.GetSecretKeys())
			{
				if (secretKey.IsSigningKey)
				{
					return secretKey;
				}
			}
		}

		throw new InvalidOperationException("The private key did not contain a signing-capable key.");
	}

	private static PgpPublicKey FindEncryptionKey(string publicKeyArmored)
	{
		using var keyStream = new MemoryStream(Encoding.UTF8.GetBytes(publicKeyArmored));
		using var decoderStream = PgpUtilities.GetDecoderStream(keyStream);
		var bundle = new PgpPublicKeyRingBundle(decoderStream);

		PgpPublicKey? fallback = null;
		foreach (PgpPublicKeyRing ring in bundle.GetKeyRings())
		{
			foreach (PgpPublicKey publicKey in ring.GetPublicKeys())
			{
				if (!publicKey.IsEncryptionKey || publicKey.HasRevocation())
				{
					continue;
				}

				// Prefer a dedicated encryption subkey over the master key.
				if (!publicKey.IsMasterKey)
				{
					return publicKey;
				}

				fallback ??= publicKey;
			}
		}

		return fallback ?? throw new InvalidOperationException("The recipient public key has no encryption-capable key.");
	}
}
