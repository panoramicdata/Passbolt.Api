namespace Passbolt.Api.Data;

/// <summary>
/// Represents a per-user encrypted secret for a resource. The <see cref="Data"/> field holds
/// an ASCII-armored OpenPGP message encrypted to the owning user's public key.
/// </summary>
public sealed class Secret : IdentifiedItem
{
	/// <summary>
	/// The identifier of the user this secret is encrypted for.
	/// </summary>
	[JsonPropertyName("user_id")]
	public string? UserId { get; set; }

	/// <summary>
	/// The identifier of the resource this secret belongs to.
	/// </summary>
	[JsonPropertyName("resource_id")]
	public string? ResourceId { get; set; }

	/// <summary>
	/// The ASCII-armored OpenPGP encrypted secret data.
	/// </summary>
	[JsonPropertyName("data")]
	public string? Data { get; set; }
}
