namespace Passbolt.Api.Requests;

/// <summary>
/// Request payload for updating a Passbolt user.
/// </summary>
public sealed class UpdateUserRequest
{
	/// <summary>
	/// Updated first name.
	/// </summary>
	[JsonPropertyName("first_name")]
	public string? FirstName { get; set; }

	/// <summary>
	/// Updated last name.
	/// </summary>
	[JsonPropertyName("last_name")]
	public string? LastName { get; set; }

	/// <summary>
	/// Updated role identifier.
	/// </summary>
	[JsonPropertyName("role_id")]
	public string? RoleId { get; set; }
}
