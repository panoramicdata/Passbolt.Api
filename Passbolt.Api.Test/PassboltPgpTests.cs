using Passbolt.Api.Cryptography;

namespace Passbolt.Api.Test;

/// <summary>
/// Deterministic (no live server) tests for the OpenPGP primitives used to protect Passbolt
/// secrets: sign+encrypt, decrypt, and public-key extraction (issue #31).
/// </summary>
public sealed class PassboltPgpTests
{
	/// <summary>
	/// A secret encrypted to and signed by a key can be decrypted back to the original plaintext.
	/// This is the create/rotate/share round trip that Passbolt relies on.
	/// </summary>
	[Fact]
	public void EncryptAndSign_ThenDecrypt_RoundTrips()
	{
		var privateKey = TestFixtures.Read("test-private.asc");
		var publicKey = PassboltPgp.ExtractPublicKey(privateKey);
		const string secret = "correct horse battery staple";

		var armored = PassboltPgp.EncryptAndSign(secret, [publicKey], privateKey, string.Empty);

		armored.Should().StartWith("-----BEGIN PGP MESSAGE-----");
		var decrypted = PassboltPgp.Decrypt(armored, privateKey, string.Empty);
		decrypted.Should().Be(secret);
	}

	/// <summary>
	/// A JSON secret payload (the shape used by the password-and-description resource type)
	/// round-trips unchanged.
	/// </summary>
	[Fact]
	public void EncryptAndSign_ThenDecrypt_RoundTrips_JsonPayload()
	{
		var privateKey = TestFixtures.Read("test-private.asc");
		var publicKey = PassboltPgp.ExtractPublicKey(privateKey);
		const string secret = "{\"password\":\"s3cr3t!\",\"description\":\"line one\\nline two\"}";

		var armored = PassboltPgp.EncryptAndSign(secret, [publicKey], privateKey, string.Empty);
		var decrypted = PassboltPgp.Decrypt(armored, privateKey, string.Empty);

		decrypted.Should().Be(secret);
	}

	/// <summary>
	/// The shared decrypt primitive recovers the plaintext of the committed fixture message.
	/// </summary>
	[Fact]
	public void Decrypt_RecoversPlaintext_FromFixture()
	{
		var privateKey = TestFixtures.Read("test-private.asc");
		var message = TestFixtures.Read("test-message.asc");
		var expected = TestFixtures.Read("test-plaintext.txt");

		PassboltPgp.Decrypt(message, privateKey, string.Empty).Should().Be(expected);
	}
}
