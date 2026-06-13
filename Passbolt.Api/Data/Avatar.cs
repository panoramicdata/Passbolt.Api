namespace Passbolt.Api.Data;

/// <summary>
/// Represents a user avatar in Passbolt.
/// </summary>
public sealed class Avatar : IdentifiedItem
{
	/// <summary>
	/// Gets or sets the ID of the user this avatar belongs to.
	/// </summary>
	public required string UserId { get; set; }

	/// <summary>
	/// Gets or sets the avatar image data (base64 encoded).
	/// </summary>
	public required string Image { get; set; }

	/// <summary>
	/// Gets or sets the avatar URL.
	/// </summary>
	public required string Url { get; set; }

	/// <summary>
	/// Gets or sets the creation timestamp.
	/// </summary>
	public required DateTime Created { get; set; }

	/// <summary>
	/// Gets or sets the last modification timestamp.
	/// </summary>
	public required DateTime Modified { get; set; }
}
