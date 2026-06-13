namespace Passbolt.Api.Requests;

/// <summary>
/// Request payload for updating a Passbolt group.
/// </summary>
public sealed class UpdateGroupRequest
{
	/// <summary>
	/// Updated group display name.
	/// </summary>
	[JsonPropertyName("name")]
	public string? Name { get; set; }

	/// <summary>
	/// Updated group user memberships.
	/// </summary>
	[JsonPropertyName("groups_users")]
	public List<GroupUserMembershipRequest> GroupUsers { get; set; } = [];
}
