namespace Passbolt.Api.Apis;

/// <summary>
/// Represents the API endpoints for accessing the current authenticated user's information.
/// </summary>
public interface IPassboltMeApi
{
	/// <summary>
	/// Gets the profile information of the currently authenticated user.
	/// </summary>
	/// <param name="cancellationToken">Cancellation token for the operation.</param>
	/// <returns>A response containing the current user's profile.</returns>
	[Get("/users/me.json")]
	Task<Response<User>> GetAsync(CancellationToken cancellationToken);
}
