namespace Passbolt.Api.Apis;

/// <summary>
/// Represents the API endpoints for managing resources in Passbolt.
/// </summary>
public interface IPassboltResourcesApi
{
	/// <summary>
	/// Lists all resources in Passbolt.
	/// </summary>
	/// <param name="cancellationToken"></param>
	/// <returns></returns>
	[Get("/resources.json")]
	Task<Response<IReadOnlyList<Resource>>> GetAllAsync(
		CancellationToken cancellationToken);

	/// <summary>
	/// Gets a specific resource by its ID.
	/// </summary>
	/// <param name="resourceId"></param>
	/// <param name="cancellationToken"></param>
	/// <returns></returns>
	[Get("/resources/{resourceId}.json")]
	Task<Response<Resource>> GetAsync(
		string resourceId,
		CancellationToken cancellationToken);

	/// <summary>
	/// Gets a specific resource by its name.
	/// </summary>
	/// <param name="name"></param>
	/// <param name="cancellationToken"></param>
	/// <returns></returns>
	[Get("/resources/byName/{name}.json")]
	Task<Response<IReadOnlyList<Resource>>> GetByNameAsync(
		string name,
		CancellationToken cancellationToken);

	/// <summary>
	/// Gets a specific resource by its Url.
	/// </summary>
	/// <param name="uri"></param>
	/// <param name="cancellationToken"></param>
	/// <returns></returns>
	[Get("/resources/byUri/{uri}.json")]
	Task<Response<IReadOnlyList<Resource>>> GetByUriAsync(
		string uri,
		CancellationToken cancellationToken);

	/// <summary>
	/// Creates a new resource in Passbolt.
	/// </summary>
	/// <param name="request"></param>
	/// <param name="cancellationToken"></param>
	/// <returns></returns>
	[Post("/resources.json")]
	Task<Response<Resource>> CreateAsync(
		[Body] CreateResourceRequest request,
		CancellationToken cancellationToken);

	/// <summary>
	/// Updates an existing resource in Passbolt.
	/// </summary>
	/// <param name="resourceId"></param>
	/// <param name="request"></param>
	/// <param name="cancellationToken"></param>
	/// <returns></returns>
	[Put("/resources/{resourceId}.json")]
	Task<Response<Resource>> UpdateAsync(
		string resourceId,
		[Body] UpdateResourceRequest request,
		CancellationToken cancellationToken);

	/// <summary>
	/// Deletes a resource in Passbolt.
	/// </summary>
	/// <param name="resourceId"></param>
	/// <param name="cancellationToken"></param>
	/// <returns></returns>
	[Delete("/resources/{resourceId}.json")]
	Task<ApiResponse<string>> DeleteAsync(
		string resourceId,
		CancellationToken cancellationToken);

	/// <summary>
	/// Shares a resource in Passbolt.
	/// </summary>
	/// <param name="resourceId"></param>
	/// <param name="request"></param>
	/// <param name="cancellationToken"></param>
	/// <returns></returns>
	[Post("/share/resource/{resourceId}.json")]
	Task<Response<Resource>> ShareAsync(
		string resourceId,
		[Body] ShareResourceRequest request,
		CancellationToken cancellationToken);
}
