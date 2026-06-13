namespace Passbolt.Api.Data;

/// <summary>
/// Represents a user avatar in Passbolt.
/// </summary>
public sealed class Avatar : IdentifiedItem
{
	/// <summary>
	/// Gets or sets the ID of the user this avatar belongs to.
	/// </summary>
	[JsonPropertyName("user_id")]
	public required string UserId { get; set; }

	/// <summary>
	/// Gets or sets the avatar image data (base64 encoded).
	/// </summary>
	[JsonPropertyName("image")]
	public required string Image { get; set; }

	/// <summary>
	/// Gets or sets the avatar URL.
	/// </summary>
	[JsonPropertyName("url")]
	public required string Url { get; set; }
}
