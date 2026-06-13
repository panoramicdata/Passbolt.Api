namespace Passbolt.Api.Data;

/// <summary>
/// Represents a Passbolt resource.
/// </summary>
public sealed class Resource : NamedIdentifiedItem
{
    /// <summary>
    /// Parent folder identifier.
    /// </summary>
	[JsonPropertyName("folder_parent_id")]
	public string? FolderParentId { get; set; }

	/// <summary>
	/// Username/email.
	/// </summary>
	[JsonPropertyName("username")]
	public string? Username { get; set; }

	/// <summary>
	/// Resource URI.
	/// </summary>
	[JsonPropertyName("uri")]
	public string? Uri { get; set; }

	/// <summary>
	/// Resource password.
	/// </summary>
	[JsonPropertyName("password")]
	public string? Password { get; set; }

	/// <summary>
	/// Resource description.
	/// </summary>
	[JsonPropertyName("description")]
	public string? Description { get; set; }

	/// <summary>
	/// Resource metadata.
	/// </summary>
	[JsonPropertyName("metadata")]
	public IReadOnlyDictionary<string, JsonElement>? Metadata { get; set; }

	/// <summary>
	/// Resource secret.
	/// </summary>
	[JsonPropertyName("secret")]
	public IReadOnlyDictionary<string, JsonElement>? Secret { get; set; }

    /// <summary>
    /// Indicates whether the resource is deleted.
    /// </summary>
	[JsonPropertyName("deleted")]
	public bool? Deleted { get; set; }

	/// <summary>
	/// Indicates whether the resource is expired.
	/// </summary>
	[JsonPropertyName("expired")]
	public bool? Expired { get; set; }

	/// <summary>
	/// Expiration timestamp when the resource is expired.
	/// </summary>
	[JsonPropertyName("expired_at")]
	public DateTimeOffset? ExpiredAt { get; set; }

	/// <summary>
	/// Resource type identifier.
	/// </summary>
	[JsonPropertyName("resource_type_id")]
	public string? ResourceTypeId { get; set; }
}
