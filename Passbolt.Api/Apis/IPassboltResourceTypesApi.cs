namespace Passbolt.Api.Apis;

/// <summary>
/// Represents the API endpoints for reading Passbolt resource types.
/// </summary>
public interface IPassboltResourceTypesApi
{
	/// <summary>
	/// Lists all resource types configured on the server.
	/// </summary>
	/// <param name="cancellationToken">Cancellation token for the operation.</param>
	/// <returns>A task containing the list of resource types.</returns>
	[Get("/resource-types.json")]
	Task<Response<IReadOnlyList<ResourceType>>> GetAllAsync(
		CancellationToken cancellationToken);
}
