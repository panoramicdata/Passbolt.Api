namespace Passbolt.Api.Requests;

/// <summary>
/// Request payload for creating a folder.
/// </summary>
public sealed class CreateFolderRequest
{
	/// <summary>
	/// Folder name.
	/// </summary>
	[JsonPropertyName("name")]
	public required string Name { get; set; }

	/// <summary>
	/// Optional parent folder identifier.
	/// </summary>
	[JsonPropertyName("folder_parent_id")]
	public string? ParentFolderId { get; set; }
}
