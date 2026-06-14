namespace Passbolt.Api.Apis;

/// <summary>
/// Represents the API endpoints for managing folders in Passbolt.
/// </summary>
public interface IPassboltFoldersApi
{
	/// <summary>
	/// Lists all folders in Passbolt.
	/// </summary>
	/// <param name="cancellationToken">Cancellation token for the operation.</param>
	/// <returns>A task containing the list of folders.</returns>
	[Get("/folders.json")]
	Task<Response<IReadOnlyList<Folder>>> ListFoldersAsync(
		CancellationToken cancellationToken);

	/// <summary>
	/// Gets a specific folder by its ID.
	/// </summary>
	/// <param name="folderId">The ID of the folder to retrieve.</param>
	/// <param name="cancellationToken">Cancellation token for the operation.</param>
	/// <returns>A task containing the folder.</returns>
	[Get("/folders/{folderId}.json")]
	Task<Response<Folder>> GetFolderAsync(
		string folderId,
		CancellationToken cancellationToken);

	/// <summary>
	/// Creates a new folder in Passbolt.
	/// </summary>
	/// <param name="request">The folder creation request.</param>
	/// <param name="cancellationToken">Cancellation token for the operation.</param>
	/// <returns>A task containing the created folder.</returns>
	[Post("/folders.json")]
	Task<Response<Folder>> CreateFolderAsync(
		[Body] CreateFolderRequest request,
		CancellationToken cancellationToken);

	/// <summary>
	/// Updates an existing folder in Passbolt.
	/// </summary>
	/// <param name="folderId">The ID of the folder to update.</param>
	/// <param name="request">The folder update request.</param>
	/// <param name="cancellationToken">Cancellation token for the operation.</param>
	/// <returns>A task containing the updated folder.</returns>
	[Put("/folders/{folderId}.json")]
	Task<Response<Folder>> UpdateFolderAsync(
		string folderId,
		[Body] UpdateFolderRequest request,
		CancellationToken cancellationToken);

	/// <summary>
	/// Deletes a folder in Passbolt.
	/// </summary>
	/// <param name="folderId">The ID of the folder to delete.</param>
	/// <param name="cancellationToken">Cancellation token for the operation.</param>
	/// <returns>A task containing the deletion response.</returns>
	[Delete("/folders/{folderId}.json")]
	Task<ApiResponse<string>> DeleteFolderAsync(
		string folderId,
		CancellationToken cancellationToken);
}
