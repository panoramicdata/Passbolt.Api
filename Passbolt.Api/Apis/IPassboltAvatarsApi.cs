namespace Passbolt.Api.Apis;

/// <summary>
/// Represents the API endpoints for accessing user avatars in Passbolt.
/// </summary>
public interface IPassboltAvatarsApi
{
	/// <summary>
	/// Gets the avatar for a specific user.
	/// </summary>
	/// <param name="userId">The ID of the user.</param>
	/// <param name="cancellationToken">Cancellation token.</param>
	/// <returns>A response containing the avatar data.</returns>
	[Get("/avatars/{userId}.json")]
	Task<Response<Avatar>> GetAsync(
		string userId,
		CancellationToken cancellationToken);
}
