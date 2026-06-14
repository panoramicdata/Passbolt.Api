namespace Passbolt.Api.Apis;

/// <summary>
/// Represents the API endpoints for managing resources in Passbolt.
/// </summary>
public interface IPassboltResourcesApi
{
	/// <summary>
	/// Lists all resources in Passbolt.
	/// </summary>
	/// <param name="cancellationToken">Cancellation token for the operation.</param>
	/// <returns>A task containing the list of resources.</returns>
	[Get("/resources.json")]
	Task<Response<IReadOnlyList<Resource>>> GetAllAsync(
		CancellationToken cancellationToken);

	/// <summary>
	/// Gets a specific resource by its ID.
	/// </summary>
	/// <param name="resourceId">The ID of the resource to retrieve.</param>
	/// <param name="cancellationToken">Cancellation token for the operation.</param>
	/// <returns>A task containing the resource.</returns>
	[Get("/resources/{resourceId}.json")]
	Task<Response<Resource>> GetAsync(
		string resourceId,
		CancellationToken cancellationToken);

	/// <summary>
	/// Gets a specific resource by its name.
	/// </summary>
	/// <param name="name">The name of the resource to retrieve.</param>
	/// <param name="cancellationToken">Cancellation token for the operation.</param>
	/// <returns>A task containing the list of matching resources.</returns>
	[Get("/resources/byName/{name}.json")]
	Task<Response<IReadOnlyList<Resource>>> GetByNameAsync(
		string name,
		CancellationToken cancellationToken);

	/// <summary>
	/// Gets a specific resource by its URI.
	/// </summary>
	/// <param name="uri">The URI of the resource to retrieve.</param>
	/// <param name="cancellationToken">Cancellation token for the operation.</param>
	/// <returns>A task containing the list of matching resources.</returns>
	[Get("/resources/byUri/{uri}.json")]
	Task<Response<IReadOnlyList<Resource>>> GetByUriAsync(
		string uri,
		CancellationToken cancellationToken);

	/// <summary>
	/// Creates a new resource in Passbolt.
	/// </summary>
	/// <param name="request">The resource creation request.</param>
	/// <param name="cancellationToken">Cancellation token for the operation.</param>
	/// <returns>A task containing the created resource.</returns>
	[Post("/resources.json")]
	Task<Response<Resource>> CreateAsync(
		[Body] CreateResourceRequest request,
		CancellationToken cancellationToken);

	/// <summary>
	/// Updates an existing resource in Passbolt.
	/// </summary>
	/// <param name="resourceId">The ID of the resource to update.</param>
	/// <param name="request">The resource update request.</param>
	/// <param name="cancellationToken">Cancellation token for the operation.</param>
	/// <returns>A task containing the updated resource.</returns>
	[Put("/resources/{resourceId}.json")]
	Task<Response<Resource>> UpdateAsync(
		string resourceId,
		[Body] UpdateResourceRequest request,
		CancellationToken cancellationToken);

	/// <summary>
	/// Deletes a resource in Passbolt.
	/// </summary>
	/// <param name="resourceId">The ID of the resource to delete.</param>
	/// <param name="cancellationToken">Cancellation token for the operation.</param>
	/// <returns>A task containing the deletion response.</returns>
	[Delete("/resources/{resourceId}.json")]
	Task<ApiResponse<string>> DeleteAsync(
		string resourceId,
		CancellationToken cancellationToken);

	/// <summary>
	/// Shares a resource in Passbolt.
	/// </summary>
	/// <param name="resourceId">The ID of the resource to share.</param>
	/// <param name="request">The resource sharing request.</param>
	/// <param name="cancellationToken">Cancellation token for the operation.</param>
	/// <returns>A task containing the shared resource.</returns>
	[Post("/share/resource/{resourceId}.json")]
	Task<Response<Resource>> ShareAsync(
		string resourceId,
		[Body] ShareResourceRequest request,
		CancellationToken cancellationToken);
}
