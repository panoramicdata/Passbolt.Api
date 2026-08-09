namespace Passbolt.Api.Requests;

/// <summary>
/// Request payload for creating a Passbolt resource. This is the raw API shape: the (encrypted)
/// secret is supplied via <see cref="Secrets"/> rather than as plaintext. Prefer the high-level
/// <see cref="PassboltClient.CreateResourceAsync"/> helper, which performs the PGP encryption and
/// resource-type resolution for you.
/// </summary>
public sealed class CreateResourceRequest
{
	/// <summary>
	/// Resource name.
	/// </summary>
	[JsonPropertyName("name")]
	public required string Name { get; set; }

	/// <summary>
	/// Resource URI.
	/// </summary>
	[JsonPropertyName("uri")]
	public string? Uri { get; set; }

	/// <summary>
	/// Resource username.
	/// </summary>
	[JsonPropertyName("username")]
	public string? Username { get; set; }

	/// <summary>
	/// Plaintext description. Only used by resource types (for example <c>password-string</c>) that
	/// keep the description unencrypted. For <c>password-and-description</c> the description is
	/// carried inside the encrypted secret instead and this is left null.
	/// </summary>
	[JsonPropertyName("description")]
	public string? Description { get; set; }

	/// <summary>
	/// The resource type identifier that determines the secret payload shape.
	/// </summary>
	[JsonPropertyName("resource_type_id")]
	public string? ResourceTypeId { get; set; }

	/// <summary>
	/// Parent folder identifier.
	/// </summary>
	[JsonPropertyName("folder_parent_id")]
	public string? ParentFolderId { get; set; }

	/// <summary>
	/// The encrypted secret(s). On create this holds exactly one entry, encrypted to the creator.
	/// </summary>
	[JsonPropertyName("secrets")]
	public List<SecretRequest> Secrets { get; set; } = [];
}
