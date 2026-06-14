namespace Passbolt.Api.Apis;

/// <summary>
/// Represents the API endpoints for checking the status of the Passbolt server.
/// </summary>
public interface IPassboltStatusApi
{
	/// <summary>
	/// Gets the status of the Passbolt server.
	/// </summary>
	/// <param name="cancellationToken">Cancellation token for the operation.</param>
	/// <returns>A task containing the server status response.</returns>
	[Get("/status.json")]
	Task<ServerStatus> GetStatusAsync(
		CancellationToken cancellationToken);

	/// <summary>
	/// Gets the healthcheck status of the Passbolt server.
	/// </summary>
	/// <param name="cancellationToken">Cancellation token for the operation.</param>
	/// <returns>A task containing the healthcheck response.</returns>
	[Get("/healthcheck/status.json")]
	Task<Response<JsonElement>> GetHealthcheckAsync(
		CancellationToken cancellationToken);
}
