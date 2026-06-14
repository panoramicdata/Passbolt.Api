namespace Passbolt.Api.Apis;

/// <summary>
/// Represents the API endpoints for managing users in Passbolt.
/// </summary>
public interface IPassboltUsersApi
{
	/// <summary>
	/// Lists all users in Passbolt.
	/// </summary>
	/// <param name="cancellationToken">Cancellation token for the operation.</param>
	/// <returns>A task containing the list of users.</returns>
	[Get("/users.json")]
	Task<Response<IReadOnlyList<User>>> GetAllAsync(
		CancellationToken cancellationToken);

	/// <summary>
	/// Gets a specific user by their ID.
	/// </summary>
	/// <param name="userId">The ID of the user to retrieve.</param>
	/// <param name="cancellationToken">Cancellation token for the operation.</param>
	/// <returns>A task containing the user.</returns>
	[Get("/users/{userId}.json")]
	Task<Response<User>> GetAsync(
		string userId,
		CancellationToken cancellationToken);

	/// <summary>
	/// Creates a new user in Passbolt.
	/// </summary>
	/// <param name="request">The user creation request.</param>
	/// <param name="cancellationToken">Cancellation token for the operation.</param>
	/// <returns>A task containing the created user.</returns>
	[Post("/users.json")]
	Task<Response<User>> CreateAsync(
		[Body] CreateUserRequest request,
		CancellationToken cancellationToken);

	/// <summary>
	/// Updates an existing user in Passbolt.
	/// </summary>
	/// <param name="userId">The ID of the user to update.</param>
	/// <param name="request">The user update request.</param>
	/// <param name="cancellationToken">Cancellation token for the operation.</param>
	/// <returns>A task containing the updated user.</returns>
	[Put("/users/{userId}.json")]
	Task<Response<User>> UpdateAsync(
		string userId,
		[Body] UpdateUserRequest request,
		CancellationToken cancellationToken);

	/// <summary>
	/// Deletes a user in Passbolt.
	/// </summary>
	/// <param name="userId">The ID of the user to delete.</param>
	/// <param name="cancellationToken">Cancellation token for the operation.</param>
	/// <returns>A task containing the deletion response.</returns>
	[Delete("/users/{userId}.json")]
	Task<ApiResponse<string>> DeleteAsync(
		string userId,
		CancellationToken cancellationToken);
}
