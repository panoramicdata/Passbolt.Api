namespace Passbolt.Api.Requests;

/// <summary>
/// Request payload for sharing a resource. Alongside the desired <see cref="Permissions"/>, the
/// secret must be re-encrypted and supplied in <see cref="Secrets"/> for every user newly granted
/// access. Prefer the high-level <see cref="PassboltClient.ShareResourceAsync"/> helper.
/// </summary>
public sealed class ShareResourceRequest
{
	/// <summary>
	/// The permission changes to apply (grants, updates and revocations).
	/// </summary>
	[JsonPropertyName("permissions")]
	public List<SharePermissionRequest> Permissions { get; set; } = [];

	/// <summary>
	/// The re-encrypted secret for each newly added recipient.
	/// </summary>
	[JsonPropertyName("secrets")]
	public List<SecretRequest> Secrets { get; set; } = [];
}
