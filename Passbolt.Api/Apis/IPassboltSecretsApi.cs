namespace Passbolt.Api.Apis;

/// <summary>
/// Represents the API endpoints for reading resource secrets for the current user.
/// </summary>
public interface IPassboltSecretsApi
{
	/// <summary>
	/// Gets the current user's encrypted secret for a resource.
	/// </summary>
	/// <param name="resourceId">The resource whose secret to retrieve.</param>
	/// <param name="cancellationToken">Cancellation token for the operation.</param>
	/// <returns>A task containing the encrypted secret.</returns>
	[Get("/secrets/resource/{resourceId}.json")]
	Task<Response<Secret>> GetForResourceAsync(
		string resourceId,
		CancellationToken cancellationToken);
}
