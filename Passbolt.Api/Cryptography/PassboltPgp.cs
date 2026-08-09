using Org.BouncyCastle.Bcpg;
using Org.BouncyCastle.Bcpg.OpenPgp;
using Org.BouncyCastle.Security;

namespace Passbolt.Api.Cryptography;

/// <summary>
/// OpenPGP primitives used by the Passbolt client to encrypt, sign and decrypt secret payloads.
/// Passbolt stores each secret as an ASCII-armored OpenPGP message encrypted to a recipient's
/// public key; when a secret is shared or rotated it must be re-encrypted once per recipient.
/// </summary>
public static class PassboltPgp
{
	private const int BufferSize = 1 << 16;

	/// <summary>
	/// Decrypts an ASCII-armored OpenPGP message using the supplied private key.
	/// </summary>
	/// <param name="armoredMessage">The ASCII-armored OpenPGP message.</param>
	/// <param name="privateKeyArmored">The ASCII-armored private key that can decrypt the message.</param>
	/// <param name="passphrase">The passphrase protecting the private key (empty string if unprotected).</param>
	/// <returns>The decrypted plaintext.</returns>
	public static string Decrypt(string armoredMessage, string privateKeyArmored, string passphrase)
	{
		ArgumentNullException.ThrowIfNull(armoredMessage);
		ArgumentNullException.ThrowIfNull(privateKeyArmored);

		using var encryptedInput = new MemoryStream(Encoding.UTF8.GetBytes(armoredMessage));
		using var decoderStream = PgpUtilities.GetDecoderStream(encryptedInput);
		var factory = new PgpObjectFactory(decoderStream);
		var encryptedDataList = factory.NextPgpObject() as PgpEncryptedDataList
			?? factory.NextPgpObject() as PgpEncryptedDataList
			?? throw new InvalidOperationException("Failed to parse the encrypted OpenPGP message.");

		var bundle = ReadSecretKeyBundle(privateKeyArmored);
		using var clearStream = GetDecryptedStream(encryptedDataList, bundle, passphrase ?? string.Empty);
		return ExtractLiteralData(clearStream);
	}

	/// <summary>
	/// Signs and encrypts the plaintext for one or more recipients, returning an ASCII-armored
	/// OpenPGP message. The message is signed with the supplied private key so that Passbolt and
	/// other OpenPGP clients accept it.
	/// </summary>
	/// <param name="plaintext">The plaintext secret to protect.</param>
	/// <param name="recipientPublicKeysArmored">The ASCII-armored public keys of every recipient.</param>
	/// <param name="signingPrivateKeyArmored">The ASCII-armored private key used to sign the message.</param>
	/// <param name="signingPassphrase">The passphrase protecting the signing key (empty string if unprotected).</param>
	/// <returns>The ASCII-armored, signed and encrypted OpenPGP message.</returns>
	public static string EncryptAndSign(
		string plaintext,
		IReadOnlyCollection<string> recipientPublicKeysArmored,
		string signingPrivateKeyArmored,
		string signingPassphrase)
	{
		ArgumentNullException.ThrowIfNull(plaintext);
		ArgumentNullException.ThrowIfNull(recipientPublicKeysArmored);
		ArgumentNullException.ThrowIfNull(signingPrivateKeyArmored);
		if (recipientPublicKeysArmored.Count == 0)
		{
			throw new ArgumentException("At least one recipient public key is required.", nameof(recipientPublicKeysArmored));
		}

		var data = Encoding.UTF8.GetBytes(plaintext);

		var signingSecretKey = FindSigningSecretKey(ReadSecretKeyBundle(signingPrivateKeyArmored));
		var signingPrivateKey = signingSecretKey.ExtractPrivateKey((signingPassphrase ?? string.Empty).ToCharArray());

		var encryptedDataGenerator = new PgpEncryptedDataGenerator(
			SymmetricKeyAlgorithmTag.Aes256,
			withIntegrityPacket: true,
			new SecureRandom());
		foreach (var recipientKey in recipientPublicKeysArmored)
		{
			encryptedDataGenerator.AddMethod(FindEncryptionKey(recipientKey));
		}

		using var armoredOutput = new MemoryStream();
		using (var armorStream = new ArmoredOutputStream(armoredOutput))
		{
			using var encryptedStream = encryptedDataGenerator.Open(armorStream, new byte[BufferSize]);

			var compressedDataGenerator = new PgpCompressedDataGenerator(CompressionAlgorithmTag.Zip);
			using var compressedStream = compressedDataGenerator.Open(encryptedStream);

			var signatureGenerator = new PgpSignatureGenerator(signingSecretKey.PublicKey.Algorithm, HashAlgorithmTag.Sha256);
			signatureGenerator.InitSign(PgpSignature.BinaryDocument, signingPrivateKey);
			foreach (var userId in signingSecretKey.PublicKey.GetUserIds())
			{
				var subpacketGenerator = new PgpSignatureSubpacketGenerator();
				subpacketGenerator.AddSignerUserId(false, userId);
				signatureGenerator.SetHashedSubpackets(subpacketGenerator.Generate());
				break;
			}

			signatureGenerator.GenerateOnePassVersion(false).Encode(compressedStream);

			var literalDataGenerator = new PgpLiteralDataGenerator();
			using (var literalStream = literalDataGenerator.Open(compressedStream, PgpLiteralData.Binary, string.Empty, data.Length, DateTime.UtcNow))
			{
				literalStream.Write(data, 0, data.Length);
				signatureGenerator.Update(data);
			}

			signatureGenerator.Generate().Encode(compressedStream);
		}

		return Encoding.ASCII.GetString(armoredOutput.ToArray());
	}

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

	private static PgpSecretKeyRingBundle ReadSecretKeyBundle(string privateKeyArmored)
	{
		using var keyStream = new MemoryStream(Encoding.UTF8.GetBytes(privateKeyArmored));
		using var decoderStream = PgpUtilities.GetDecoderStream(keyStream);
		return new PgpSecretKeyRingBundle(decoderStream);
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
				if (!publicKey.IsEncryptionKey || publicKey.IsRevoked())
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

	private static Stream GetDecryptedStream(PgpEncryptedDataList encryptedDataList, PgpSecretKeyRingBundle bundle, string passphrase)
	{
		foreach (var encryptedData in encryptedDataList.GetEncryptedDataObjects().OfType<PgpPublicKeyEncryptedData>())
		{
			var secretKey = bundle.GetSecretKey(encryptedData.KeyId);
			if (secretKey is null)
			{
				continue;
			}

			var privateKey = secretKey.ExtractPrivateKey(passphrase.ToCharArray());
			return encryptedData.GetDataStream(privateKey);
		}

		throw new InvalidOperationException("No matching private key was found to decrypt the OpenPGP message.");
	}

	private static string ExtractLiteralData(Stream clearStream)
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

			while (pgpObject is not null and not PgpLiteralData)
			{
				if (pgpObject is PgpOnePassSignatureList or PgpSignatureList or PgpMarker)
				{
					pgpObject = factory.NextPgpObject();
					continue;
				}

				throw new InvalidOperationException($"The OpenPGP message contained an unsupported payload type {pgpObject.GetType().Name}.");
			}

			if (pgpObject is not PgpLiteralData literalData)
			{
				throw new InvalidOperationException("The OpenPGP message did not contain a literal data packet.");
			}

			using var literalStream = literalData.GetInputStream();
			using var reader = new StreamReader(literalStream, Encoding.UTF8);
			return reader.ReadToEnd();
		}
		finally
		{
			decompressedStream?.Dispose();
		}
	}
}
