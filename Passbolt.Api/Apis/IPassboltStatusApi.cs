namespace Passbolt.Api.Apis;

/// <summary>
/// Represents the API endpoints for checking the status of the Passbolt server.
/// </summary>
public interface IPassboltStatusApi
{
	/// <summary>
	/// Gets the status of the Passbolt server.
	/// </summary>
	/// <param name="cancellationToken"></param>
	/// <returns></returns>
	[Get("/status.json")]
	Task<ServerStatus> GetStatusAsync(
		CancellationToken cancellationToken);

	/// <summary>
	/// Gets the healthcheck status of the Passbolt server.
	/// </summary>
	/// <param name="cancellationToken"></param>
	/// <returns></returns>
	[Get("/healthcheck/status.json")]
	Task<Response<JsonElement>> GetHealthcheckAsync(
		CancellationToken cancellationToken);
}
