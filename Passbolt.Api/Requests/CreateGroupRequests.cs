namespace Passbolt.Api.Requests;

/// <summary>
/// Request payload for creating a Passbolt group.
/// </summary>
public sealed class CreateGroupRequest
{
	/// <summary>
	/// Group display name.
	/// </summary>
	[JsonPropertyName("name")]
	public required string Name { get; set; }

	/// <summary>
	/// Initial group users.
	/// </summary>
	[JsonPropertyName("groups_users")]
	public List<GroupUserMembershipRequest> GroupUsers { get; set; } = [];
}
