namespace Passbolt.Api.Requests;

/// <summary>
/// Request payload for updating a Passbolt folder.
/// </summary>
public sealed class UpdateFolderRequest
{
	/// <summary>
	/// Folder name.
	/// </summary>
	[JsonPropertyName("name")]
	public string? Name { get; set; }

	/// <summary>
	/// Folder description.
	/// </summary>
	[JsonPropertyName("description")]
	public string? Description { get; set; }
}
