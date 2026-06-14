namespace Passbolt.Api.Apis;

/// <summary>
/// Represents the API endpoints for managing comments on resources in Passbolt.
/// </summary>
public interface IPassboltCommentsApi
{
	/// <summary>
	/// Lists all comments for a specific resource.
	/// </summary>
	/// <param name="resourceId">The ID of the resource to list comments for.</param>
	/// <param name="cancellationToken">Cancellation token for the operation.</param>
	/// <returns>A response containing a collection of comments.</returns>
	[Get("/comments?filter[resource_id]={resourceId}")]
	Task<Response<IReadOnlyList<Comment>>> GetByResourceAsync(
		string resourceId,
		CancellationToken cancellationToken);

	/// <summary>
	/// Gets a specific comment by its ID.
	/// </summary>
	/// <param name="commentId">The ID of the comment.</param>
	/// <param name="cancellationToken">Cancellation token.</param>
	/// <returns>A response containing the comment.</returns>
	[Get("/comments/{commentId}.json")]
	Task<Response<Comment>> GetAsync(
		string commentId,
		CancellationToken cancellationToken);

	/// <summary>
	/// Creates a new comment on a resource.
	/// </summary>
	/// <param name="request">The comment creation request.</param>
	/// <param name="cancellationToken">Cancellation token.</param>
	/// <returns>A response containing the created comment.</returns>
	[Post("/comments.json")]
	Task<Response<Comment>> CreateAsync(
		[Body] CreateCommentRequest request,
		CancellationToken cancellationToken);

	/// <summary>
	/// Deletes a comment.
	/// </summary>
	/// <param name="commentId">The ID of the comment to delete.</param>
	/// <param name="cancellationToken">Cancellation token.</param>
	/// <returns>An API response indicating success or failure.</returns>
	[Delete("/comments/{commentId}.json")]
	Task<ApiResponse<string>> DeleteAsync(
		string commentId,
		CancellationToken cancellationToken);
}
