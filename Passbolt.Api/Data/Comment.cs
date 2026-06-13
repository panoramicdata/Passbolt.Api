namespace Passbolt.Api.Data;

/// <summary>
/// Represents a comment on a resource in Passbolt.
/// </summary>
public sealed class Comment : IdentifiedItem
{
	/// <summary>
	/// Gets or sets the ID of the resource this comment belongs to.
	/// </summary>
	public required string ResourceId { get; set; }

	/// <summary>
	/// Gets or sets the ID of the user who created this comment.
	/// </summary>
	public required string UserId { get; set; }

	/// <summary>
	/// Gets or sets the content of the comment.
	/// </summary>
	public required string Content { get; set; }

	/// <summary>
	/// Gets or sets the creation timestamp.
	/// </summary>
	public required DateTime Created { get; set; }

	/// <summary>
	/// Gets or sets the last modification timestamp.
	/// </summary>
	public required DateTime Modified { get; set; }

	/// <summary>
	/// Gets or sets the user who created this comment.
	/// </summary>
	public User? Creator { get; set; }
}
