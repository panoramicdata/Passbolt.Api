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
	/// Lists all users including their OpenPGP public keys. Used when recipients' keys are
	/// required to encrypt resource secrets.
	/// </summary>
	/// <param name="containGpgkey">Set to 1 to include each user's gpgkey.</param>
	/// <param name="cancellationToken">Cancellation token for the operation.</param>
	/// <returns>A task containing the list of users with gpgkeys.</returns>
	[Get("/users.json")]
	Task<Response<IReadOnlyList<User>>> GetAllWithGpgKeysAsync(
		[AliasAs("contain[gpgkey]")] int containGpgkey,
		CancellationToken cancellationToken);

	/// <summary>
	/// Lists the users who have access to a given resource (expanding group membership
	/// server-side), including their OpenPGP public keys. This is the authoritative recipient
	/// set for re-encrypting a shared secret.
	/// </summary>
	/// <param name="resourceId">The resource whose accessors to resolve.</param>
	/// <param name="containGpgkey">Set to 1 to include each user's gpgkey.</param>
	/// <param name="cancellationToken">Cancellation token for the operation.</param>
	/// <returns>A task containing the users with access and their gpgkeys.</returns>
	[Get("/users.json")]
	Task<Response<IReadOnlyList<User>>> GetWithAccessToResourceAsync(
		[AliasAs("filter[has-access][]")] string resourceId,
		[AliasAs("contain[gpgkey]")] int containGpgkey,
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
