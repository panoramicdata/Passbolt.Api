namespace Passbolt.Api.Apis;

/// <summary>
/// Represents the API endpoints for managing groups in Passbolt.
/// </summary>
public interface IPassboltGroupsApi
{
	/// <summary>
	/// Lists all groups in Passbolt.
	/// </summary>
	/// <param name="cancellationToken">Cancellation token for the operation.</param>
	/// <returns>A task containing the list of groups.</returns>
	[Get("/groups.json")]
	Task<Response<IReadOnlyList<Group>>> ListAsync(CancellationToken cancellationToken);

	/// <summary>
	/// Gets a specific group by its ID.
	/// </summary>
	/// <param name="groupId">The ID of the group to retrieve.</param>
	/// <param name="cancellationToken">Cancellation token for the operation.</param>
	/// <returns>A task containing the group.</returns>
	[Get("/groups/{groupId}.json")]
	Task<Response<Group>> GetAsync(
		string groupId,
		CancellationToken cancellationToken);

	/// <summary>
	/// Creates a new group in Passbolt.
	/// </summary>
	/// <param name="request">The group creation request.</param>
	/// <param name="cancellationToken">Cancellation token for the operation.</param>
	/// <returns>A task containing the created group.</returns>
	[Post("/groups.json")]
	Task<Response<Group>> CreateAsync(
		[Body] CreateGroupRequest request,
		CancellationToken cancellationToken);

	/// <summary>
	/// Updates an existing group in Passbolt.
	/// </summary>
	/// <param name="groupId">The ID of the group to update.</param>
	/// <param name="request">The group update request.</param>
	/// <param name="cancellationToken">Cancellation token for the operation.</param>
	/// <returns>A task containing the updated group.</returns>
	[Put("/groups/{groupId}.json")]
	Task<Response<Group>> UpdateAsync(
		string groupId,
		[Body] UpdateGroupRequest request,
		CancellationToken cancellationToken);

	/// <summary>
	/// Deletes a group in Passbolt.
	/// </summary>
	/// <param name="groupId">The ID of the group to delete.</param>
	/// <param name="cancellationToken">Cancellation token for the operation.</param>
	/// <returns>A task containing the deletion response.</returns>
	[Delete("/groups/{groupId}.json")]
	Task<ApiResponse<string>> DeleteAsync(
		string groupId,
		CancellationToken cancellationToken);
}
