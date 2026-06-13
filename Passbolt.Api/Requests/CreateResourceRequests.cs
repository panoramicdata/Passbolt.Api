namespace Passbolt.Api.Requests;

/// <summary>
/// Request payload for creating a Passbolt resource.
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
	/// Resource secret/password.
	/// </summary>
	[JsonPropertyName("secret")]
	public required string Secret { get; set; }

	/// <summary>
	/// Optional resource description.
	/// </summary>
	[JsonPropertyName("description")]
	public string? Description { get; set; }

	/// <summary>
	/// Parent folder identifier.
	/// </summary>
	[JsonPropertyName("folder_parent_id")]
	public string? ParentFolderId { get; set; }
}
