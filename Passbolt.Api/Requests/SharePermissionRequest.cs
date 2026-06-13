namespace Passbolt.Api.Requests;

/// <summary>
/// Permission entry for resource sharing.
/// </summary>
public sealed class SharePermissionRequest
{
	/// <summary>
	/// User identifier for sharing (optional if group sharing is used).
	/// </summary>
	[JsonPropertyName("user_id")]
	public string? UserId { get; set; }

	/// <summary>
	/// Group identifier for sharing (optional if user sharing is used).
	/// </summary>
	[JsonPropertyName("group_id")]
	public string? GroupId { get; set; }

	/// <summary>
	/// Permission type. 1=Read, 7=Update, 15=Owner.
	/// </summary>
	[JsonPropertyName("type")]
	public int Type { get; set; } = 1;
}
