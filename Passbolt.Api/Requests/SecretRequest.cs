namespace Passbolt.Api.Requests;

/// <summary>
/// A single per-recipient encrypted secret submitted when creating, updating or sharing a
/// resource. <see cref="Data"/> is an ASCII-armored OpenPGP message encrypted to the recipient's
/// public key. On create the array holds exactly one entry (for the current user) and
/// <see cref="UserId"/> is omitted; on update/share there is one entry per recipient.
/// </summary>
public sealed class SecretRequest
{
	/// <summary>
	/// The identifier of the recipient user. Omitted when creating a resource (the single secret
	/// implicitly belongs to the current user).
	/// </summary>
	[JsonPropertyName("user_id")]
	public string? UserId { get; set; }

	/// <summary>
	/// The ASCII-armored OpenPGP encrypted secret data.
	/// </summary>
	[JsonPropertyName("data")]
	public required string Data { get; set; }
}
