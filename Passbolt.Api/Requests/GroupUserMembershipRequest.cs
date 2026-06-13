namespace Passbolt.Api.Requests;

/// <summary>
/// Group membership request record.
/// </summary>
public sealed class GroupUserMembershipRequest
{
	/// <summary>
	/// User identifier.
	/// </summary>
	[JsonPropertyName("user_id")]
	public required string UserId { get; set; }

	/// <summary>
	/// Marks the user as a group manager.
	/// </summary>
	[JsonPropertyName("is_admin")]
	public bool IsAdmin { get; set; }
}
