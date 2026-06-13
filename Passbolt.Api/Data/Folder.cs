namespace Passbolt.Api.Data;

/// <summary>
/// Represents a Passbolt folder.
/// </summary>
public sealed class Folder : NamedIdentifiedItem
{
	/// <summary>
	/// Parent folder identifier.
	/// </summary>
	[JsonPropertyName("folder_parent_id")]
	public string? FolderParentId { get; set; }

	/// <summary>
	/// Indicates whether this is a personal folder.
	/// </summary>
	[JsonPropertyName("personal")]
	public bool Personal { get; set; }

	/// <summary>
	/// Folder permissions when included in the response payload.
	/// </summary>
	[JsonPropertyName("permissions")]
	public IReadOnlyList<Permission>? Permissions { get; set; }
}
