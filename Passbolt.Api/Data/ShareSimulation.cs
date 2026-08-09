namespace Passbolt.Api.Data;

/// <summary>
/// The result of simulating a share operation. Passbolt reports which recipients would gain or
/// lose access so the caller knows exactly whose secrets must be provided.
/// </summary>
public sealed class ShareSimulationResult
{
	/// <summary>
	/// The recipient changes the share would produce.
	/// </summary>
	[JsonPropertyName("changes")]
	public ShareSimulationChanges? Changes { get; set; }
}

/// <summary>
/// The set of recipient changes a simulated share would produce.
/// </summary>
public sealed class ShareSimulationChanges
{
	/// <summary>
	/// Recipients that would newly gain access and therefore require an encrypted secret.
	/// </summary>
	[JsonPropertyName("added")]
	public IReadOnlyList<ShareSimulationSecret>? Added { get; set; }

	/// <summary>
	/// Recipients that would lose access.
	/// </summary>
	[JsonPropertyName("removed")]
	public IReadOnlyList<ShareSimulationSecret>? Removed { get; set; }
}

/// <summary>
/// Identifies a recipient affected by a simulated share.
/// </summary>
public sealed class ShareSimulationSecret
{
	/// <summary>
	/// The affected user identifier.
	/// </summary>
	[JsonPropertyName("user_id")]
	public string? UserId { get; set; }

	/// <summary>
	/// The resource identifier.
	/// </summary>
	[JsonPropertyName("resource_id")]
	public string? ResourceId { get; set; }
}
