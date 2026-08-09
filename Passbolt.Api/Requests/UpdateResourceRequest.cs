namespace Passbolt.Api.Requests;

/// <summary>
/// Request payload for updating a Passbolt resource. Metadata fields (name/uri/username/
/// description) may be updated on their own; rotating the secret additionally requires
/// <see cref="Secrets"/> to contain one re-encrypted entry for every user with access. Prefer the
/// high-level <see cref="PassboltClient.RotateResourceSecretAsync"/> helper for secret rotation.
/// </summary>
public sealed class UpdateResourceRequest
{
	/// <summary>
	/// Resource name.
	/// </summary>
	[JsonPropertyName("name")]
	public string? Name { get; set; }

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
	/// Plaintext description (only for resource types that keep the description unencrypted).
	/// </summary>
	[JsonPropertyName("description")]
	public string? Description { get; set; }

	/// <summary>
	/// The resource type identifier. Required when rotating the secret.
	/// </summary>
	[JsonPropertyName("resource_type_id")]
	public string? ResourceTypeId { get; set; }

	/// <summary>
	/// Re-encrypted secrets, one per user with access. Null/empty when only metadata is updated.
	/// </summary>
	[JsonPropertyName("secrets")]
	public List<SecretRequest>? Secrets { get; set; }
}
