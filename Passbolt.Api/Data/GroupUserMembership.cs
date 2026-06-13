namespace Passbolt.Api.Data;

/// <summary>
/// Represents group membership details for a user.
/// </summary>
public sealed class GroupUserMembership : IdentifiedItem
{
	/// <summary>
	/// Username/email.
	/// </summary>
	[JsonPropertyName("username")]
	public string? Username { get; set; }

	/// <summary>
	/// User first name.
	/// </summary>
	[JsonPropertyName("first_name")]
	public string? FirstName { get; set; }

	/// <summary>
	/// User last name.
	/// </summary>
	[JsonPropertyName("last_name")]
	public string? LastName { get; set; }

	/// <summary>
	/// Indicates whether the user is a group manager.
	/// </summary>
	[JsonPropertyName("is_group_manager")]
	public bool? IsGroupManager { get; set; }
}
