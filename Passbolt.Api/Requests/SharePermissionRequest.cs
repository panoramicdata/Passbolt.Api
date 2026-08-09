namespace Passbolt.Api.Requests;

/// <summary>
/// A single permission entry in a share request, using Passbolt's ARO/ACO model. To grant a new
/// permission set <see cref="IsNew"/>, <see cref="Aro"/>, <see cref="AroForeignKey"/>,
/// <see cref="Aco"/>, <see cref="AcoForeignKey"/> and <see cref="Type"/>. To change an existing
/// permission set <see cref="Id"/> and <see cref="Type"/>. To revoke one set <see cref="Id"/> and
/// <see cref="Delete"/>.
/// </summary>
public sealed class SharePermissionRequest
{
	/// <summary>
	/// The identifier of an existing permission (for update or delete). Null for a new grant.
	/// </summary>
	[JsonPropertyName("id")]
	public string? Id { get; set; }

	/// <summary>
	/// The access-request-object type: <c>User</c> or <c>Group</c>.
	/// </summary>
	[JsonPropertyName("aro")]
	public string? Aro { get; set; }

	/// <summary>
	/// The identifier of the user or group being granted access.
	/// </summary>
	[JsonPropertyName("aro_foreign_key")]
	public string? AroForeignKey { get; set; }

	/// <summary>
	/// The access-control-object type: <c>Resource</c>.
	/// </summary>
	[JsonPropertyName("aco")]
	public string? Aco { get; set; }

	/// <summary>
	/// The identifier of the resource being shared.
	/// </summary>
	[JsonPropertyName("aco_foreign_key")]
	public string? AcoForeignKey { get; set; }

	/// <summary>
	/// Permission type. 1 = Read, 7 = Update, 15 = Owner.
	/// </summary>
	[JsonPropertyName("type")]
	public int? Type { get; set; }

	/// <summary>
	/// Set to true when granting a brand-new permission.
	/// </summary>
	[JsonPropertyName("is_new")]
	public bool? IsNew { get; set; }

	/// <summary>
	/// Set to true to revoke an existing permission.
	/// </summary>
	[JsonPropertyName("delete")]
	public bool? Delete { get; set; }
}
