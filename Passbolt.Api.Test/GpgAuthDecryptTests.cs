using System.Reflection;

namespace Passbolt.Api.Test;

/// <summary>
/// Deterministic (no live server) regression test for the GPGAuth challenge-token decryption
/// (issue #32). Uses a throwaway PGP key and a signed+encrypted message committed as fixtures.
/// Before the fix, decryption failed with "Unable to decrypt Passbolt auth token ..." because the
/// underlying ciphertext stream was disposed before the lazy read.
/// </summary>
public sealed class GpgAuthDecryptTests
{
	private static string ReadFixture(string suffix)
	{
		var assembly = typeof(GpgAuthDecryptTests).Assembly;
		var name = assembly.GetManifestResourceNames()
			.Single(n => n.EndsWith(suffix, StringComparison.Ordinal));
		using var stream = assembly.GetManifestResourceStream(name)!;
		using var reader = new StreamReader(stream);
		return reader.ReadToEnd();
	}

	/// <summary>
	/// Decrypts a signed+encrypted armored message with the matching private key and returns the
	/// original plaintext (the shape of a Passbolt GPGAuth user auth token).
	/// </summary>
	[Fact]
	public void DecryptAuthToken_RecoversPlaintext_FromSignedEncryptedMessage()
	{
		var privateKey = ReadFixture("test-private.asc");
		var message = ReadFixture("test-message.asc");
		var expected = ReadFixture("test-plaintext.txt");

		// The fixture key is unprotected, so the passphrase is empty.
		var result = AuthenticatedLoggingHttpHandler.DecryptAuthToken(message, privateKey, string.Empty);

		result.Should().Be(expected);
	}
}
