namespace Passbolt.Api.Requests;

/// <summary>
/// Request payload for creating a Passbolt user.
/// </summary>
public sealed class CreateUserRequest
{
	/// <summary>
	/// Email/username for the user.
	/// </summary>
	[JsonPropertyName("username")]
	public required string Username { get; set; }

	/// <summary>
	/// User first name.
	/// </summary>
	[JsonPropertyName("first_name")]
	public required string FirstName { get; set; }

	/// <summary>
	/// User last name.
	/// </summary>
	[JsonPropertyName("last_name")]
	public required string LastName { get; set; }

	/// <summary>
	/// Passbolt role identifier.
	/// </summary>
	[JsonPropertyName("role_id")]
	public string? RoleId { get; set; }
}
