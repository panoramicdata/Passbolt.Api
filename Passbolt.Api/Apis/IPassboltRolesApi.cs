namespace Passbolt.Api.Apis;

/// <summary>
/// Represents the API endpoints for querying available roles in Passbolt.
/// </summary>
public interface IPassboltRolesApi
{
	/// <summary>
	/// Lists all available roles.
	/// </summary>
	/// <param name="cancellationToken">Cancellation token.</param>
	/// <returns>A response containing a collection of available roles.</returns>
	[Get("/roles.json")]
	Task<Response<IReadOnlyList<Role>>> GetAllAsync(
		CancellationToken cancellationToken);

	/// <summary>
	/// Gets a specific role by its ID.
	/// </summary>
	/// <param name="roleId">The ID of the role.</param>
	/// <param name="cancellationToken">Cancellation token.</param>
	/// <returns>A response containing the role.</returns>
	[Get("/roles/{roleId}.json")]
	Task<Response<Role>> GetAsync(
		string roleId,
		CancellationToken cancellationToken);
}
