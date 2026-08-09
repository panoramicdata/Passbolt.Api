namespace Passbolt.Api.Data;

/// <summary>
/// Represents a user's OpenPGP public key as stored in Passbolt.
/// </summary>
public sealed class GpgKey : IdentifiedItem
{
	/// <summary>
	/// The ASCII-armored public key block. This is the recipient key used to encrypt secrets.
	/// </summary>
	[JsonPropertyName("armored_key")]
	public string? ArmoredKey { get; set; }

	/// <summary>
	/// The full key fingerprint.
	/// </summary>
	[JsonPropertyName("fingerprint")]
	public string? Fingerprint { get; set; }

	/// <summary>
	/// The short/long key identifier.
	/// </summary>
	[JsonPropertyName("key_id")]
	public string? KeyId { get; set; }

	/// <summary>
	/// The identifier of the user who owns this key.
	/// </summary>
	[JsonPropertyName("user_id")]
	public string? UserId { get; set; }

	/// <summary>
	/// The key type (for example RSA).
	/// </summary>
	[JsonPropertyName("type")]
	public string? Type { get; set; }

	/// <summary>
	/// The key length in bits.
	/// </summary>
	[JsonPropertyName("bits")]
	public int? Bits { get; set; }

	/// <summary>
	/// Whether the key has been deleted.
	/// </summary>
	[JsonPropertyName("deleted")]
	public bool? Deleted { get; set; }
}
