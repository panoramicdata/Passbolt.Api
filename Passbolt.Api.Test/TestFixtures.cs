namespace Passbolt.Api.Test;

/// <summary>
/// Reads the embedded PGP fixtures (throwaway key, signed+encrypted message, expected plaintext)
/// that the deterministic crypto tests share, so no test carries its own resource-loading code.
/// </summary>
internal static class TestFixtures
{
	/// <summary>Reads the embedded fixture whose resource name ends with <paramref name="suffix"/>.</summary>
	public static string Read(string suffix)
	{
		var assembly = typeof(TestFixtures).Assembly;
		var name = assembly.GetManifestResourceNames()
			.Single(n => n.EndsWith(suffix, StringComparison.Ordinal));
		using var stream = assembly.GetManifestResourceStream(name)!;
		using var reader = new StreamReader(stream);
		return reader.ReadToEnd();
	}
}
