namespace Passbolt.Api.Requests;

/// <summary>
/// Request payload for sharing a resource.
/// </summary>
public sealed class ShareResourceRequest
{
	/// <summary>
	/// Permission records.
	/// </summary>
	[JsonPropertyName("permissions")]
	public List<SharePermissionRequest> Permissions { get; set; } = [];
}
