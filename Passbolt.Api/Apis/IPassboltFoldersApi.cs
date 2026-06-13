namespace Passbolt.Api.Apis;

/// <summary>
/// Represents the API endpoints for managing folders in Passbolt.
/// </summary>
public interface IPassboltFoldersApi
{
	/// <summary>
	/// Lists all folders in Passbolt.
	/// </summary>
	/// <param name="cancellationToken"></param>
	/// <returns></returns>
	[Get("/folders.json")]
	Task<Response<IReadOnlyList<Folder>>> ListFoldersAsync(
		CancellationToken cancellationToken);

	/// <summary>
	/// Gets a specific folder by its ID.
	/// </summary>
	/// <param name="folderId"></param>
	/// <param name="cancellationToken"></param>
	/// <returns></returns>
	[Get("/folders/{folderId}.json")]
	Task<Response<Folder>> GetFolderAsync(
		string folderId,
		CancellationToken cancellationToken);

	/// <summary>
	/// Creates a new folder in Passbolt.
	/// </summary>
	/// <param name="request"></param>
	/// <param name="cancellationToken"></param>
	/// <returns></returns>
	[Post("/folders.json")]
	Task<Response<Folder>> CreateFolderAsync(
		[Body] CreateFolderRequest request,
		CancellationToken cancellationToken);

	/// <summary>
	/// Updates an existing folder in Passbolt.
	/// </summary>
	/// <param name="folderId"></param>
	/// <param name="request"></param>
	/// <param name="cancellationToken"></param>
	/// <returns></returns>
	[Put("/folders/{folderId}.json")]
	Task<Response<Folder>> UpdateFolderAsync(
		string folderId,
		[Body] UpdateFolderRequest request,
		CancellationToken cancellationToken);

	/// <summary>
	/// Deletes a folder in Passbolt.
	/// </summary>
	/// <param name="folderId"></param>
	/// <param name="cancellationToken"></param>
	/// <returns></returns>
	[Delete("/folders/{folderId}.json")]
	Task<ApiResponse<string>> DeleteFolderAsync(
		string folderId,
		CancellationToken cancellationToken);
}
