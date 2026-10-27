namespace Passbolt.Api.Mfa;

/// <summary>
/// Recognises the response Passbolt gives to a signed-in request that still needs a second factor:
/// a 403 whose body lists the providers, for example <c>{"providers":{"totp":".../mfa/verify/totp.json"}}</c>.
/// </summary>
internal static class PassboltMfaChallenge
{
	/// <summary>
	/// Whether the body is an MFA challenge that offers a TOTP provider.
	/// </summary>
	/// <param name="responseBody">The JSON response body, with or without Passbolt's header and body envelope.</param>
	internal static bool OffersTotp(string? responseBody)
	{
		if (string.IsNullOrWhiteSpace(responseBody))
		{
			return false;
		}

		try
		{
			using var document = JsonDocument.Parse(responseBody);
			var root = document.RootElement;
			if (root.ValueKind == JsonValueKind.Object && root.TryGetProperty("body", out var body))
			{
				root = body;
			}

			return root.ValueKind == JsonValueKind.Object
				&& root.TryGetProperty("providers", out var providers)
				&& providers.ValueKind == JsonValueKind.Object
				&& providers.TryGetProperty("totp", out _);
		}
		catch (JsonException)
		{
			return false;
		}
	}
}