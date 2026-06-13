namespace Passbolt.Api.Data;

/// <summary>
/// Represents a comment on a resource in Passbolt.
/// </summary>
public sealed class Comment : IdentifiedItem
{
	/// <summary>
	/// Gets or sets the ID of the resource this comment belongs to.
	/// </summary>
	[JsonPropertyName("resource_id")]
	public required string ResourceId { get; set; }

	/// <summary>
	/// Gets or sets the ID of the user who created this comment.
	/// </summary>
	[JsonPropertyName("user_id")]
	public required string UserId { get; set; }

	/// <summary>
	/// Gets or sets the content of the comment.
	/// </summary>
	[JsonPropertyName("content")]
	public required string Content { get; set; }

	/// <summary>
	/// Gets or sets the user who created this comment.
	/// </summary>
	[JsonPropertyName("creator")]
	public User? Creator { get; set; }
}
