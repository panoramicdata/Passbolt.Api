namespace Passbolt.Api.Data;

/// <summary>
/// Represents a request to create a comment on a resource.
/// </summary>
public sealed class CreateCommentRequest
{
	/// <summary>
	/// Gets or sets the ID of the resource to comment on.
	/// </summary>
	public required string ResourceId { get; set; }

	/// <summary>
	/// Gets or sets the comment content.
	/// </summary>
	public required string Content { get; set; }
}
