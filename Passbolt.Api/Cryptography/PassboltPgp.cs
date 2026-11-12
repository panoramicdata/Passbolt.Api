using Org.BouncyCastle.Bcpg;
using Org.BouncyCastle.Bcpg.OpenPgp;
using Org.BouncyCastle.Security;

namespace Passbolt.Api.Cryptography;

/// <summary>
/// OpenPGP primitives used by the Passbolt client to encrypt, sign and decrypt secret payloads.
/// Passbolt stores each secret as an ASCII-armored OpenPGP message encrypted to a recipient's
/// public key; when a secret is shared or rotated it must be re-encrypted once per recipient.
/// </summary>
public static partial class PassboltPgp
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
		var encryptedDataList = ReadEncryptedDataList(decoderStream, "Failed to parse the encrypted OpenPGP message.");

		var bundle = ReadSecretKeyBundle(privateKeyArmored);
		using var clearStream = GetDecryptedStream(encryptedDataList, bundle, passphrase ?? string.Empty);
		return ExtractLiteralData(clearStream, "The OpenPGP message", "The OpenPGP message contained an unsupported payload type");
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
}
