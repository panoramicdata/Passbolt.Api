namespace Passbolt.Api.Apis;

/// <summary>
/// Represents the API endpoints for querying permissions in Passbolt.
/// </summary>
public interface IPassboltPermissionsApi
{
	/// <summary>
	/// Lists all permissions.
	/// </summary>
	/// <param name="cancellationToken">Cancellation token for the operation.</param>
	/// <returns>A response containing a collection of permissions.</returns>
	[Obsolete("Passbolt serves no /permissions.json endpoint: this always returns 404. List a resource's permissions with GetByResourceAsync.")]
	[Get("/permissions.json")]
	Task<Response<IReadOnlyList<Permission>>> GetAllAsync(
		CancellationToken cancellationToken);

	/// <summary>
	/// Gets a specific permission by its ID.
	/// </summary>
	/// <param name="permissionId">The ID of the permission.</param>
	/// <param name="cancellationToken">Cancellation token for the operation.</param>
	/// <returns>A response containing the permission.</returns>
	[Get("/permissions/{permissionId}.json")]
	Task<Response<Permission>> GetAsync(
		string permissionId,
		CancellationToken cancellationToken);

	/// <summary>
	/// Lists all permissions for a specific resource.
	/// </summary>
	/// <param name="resourceId">The ID of the resource.</param>
	/// <param name="cancellationToken">Cancellation token for the operation.</param>
	/// <returns>A response containing a collection of permissions for the resource.</returns>
	[Get("/permissions/resource/{resourceId}.json")]
	Task<Response<IReadOnlyList<Permission>>> GetByResourceAsync(
		string resourceId,
		CancellationToken cancellationToken);

	/// <summary>
	/// Lists all permissions for a specific user.
	/// </summary>
	/// <param name="userId">The ID of the user.</param>
	/// <param name="cancellationToken">Cancellation token for the operation.</param>
	/// <returns>A response containing a collection of permissions for the user.</returns>
	[Obsolete("Passbolt has no per-user permissions endpoint: this always returns 404. List a resource's permissions with GetByResourceAsync.")]
	[Get("/permissions?filter[user_id]={userId}")]
	Task<Response<IReadOnlyList<Permission>>> GetByUserAsync(
		string userId,
		CancellationToken cancellationToken);
}
