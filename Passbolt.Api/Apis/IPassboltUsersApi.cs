namespace Passbolt.Api.Apis;

/// <summary>
/// Represents the API endpoints for managing users in Passbolt.
/// </summary>
public interface IPassboltUsersApi
{
	/// <summary>
	/// Lists all users in Passbolt.
	/// </summary>
	/// <param name="cancellationToken"></param>
	/// <returns></returns>
	[Get("/users.json")]
	Task<Response<IReadOnlyList<User>>> GetAllAsync(
		CancellationToken cancellationToken);

	/// <summary>
	/// Gets a specific user by their ID.
	/// </summary>
	/// <param name="userId"></param>
	/// <param name="cancellationToken"></param>
	/// <returns></returns>
	[Get("/users/{userId}.json")]
	Task<Response<User>> GetAsync(
		string userId,
		CancellationToken cancellationToken);

	/// <summary>
	/// Creates a new user in Passbolt.
	/// </summary>
	/// <param name="request"></param>
	/// <param name="cancellationToken"></param>
	/// <returns></returns>
	[Post("/users.json")]
	Task<Response<User>> CreateAsync(
		[Body] CreateUserRequest request,
		CancellationToken cancellationToken);

	/// <summary>
	/// Updates an existing user in Passbolt.
	/// </summary>
	/// <param name="userId"></param>
	/// <param name="request"></param>
	/// <param name="cancellationToken"></param>
	/// <returns></returns>
	[Put("/users/{userId}.json")]
	Task<Response<User>> UpdateAsync(
		string userId,
		[Body] UpdateUserRequest request,
		CancellationToken cancellationToken);

	/// <summary>
	/// Deletes a user in Passbolt.
	/// </summary>
	/// <param name="userId"></param>
	/// <param name="cancellationToken"></param>
	/// <returns></returns>
	[Delete("/users/{userId}.json")]
	Task<ApiResponse<string>> DeleteAsync(
		string userId,
		CancellationToken cancellationToken);
}
