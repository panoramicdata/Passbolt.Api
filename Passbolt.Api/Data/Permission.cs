namespace Passbolt.Api.Data;

/// <summary>
/// Represents a Passbolt permission entry.
/// </summary>
public sealed class Permission : IdentifiedItem
{
    /// <summary>
    /// Access Control Object.
    /// </summary>
	[JsonPropertyName("aco")]
	public string? Aco { get; set; }

    /// <summary>
    /// Access Control Object foreign key.
    /// </summary>
	[JsonPropertyName("aco_foreign_key")]
	public string? AcoForeignKey { get; set; }

	/// <summary>
	/// Access Request Object.
	/// </summary>
	[JsonPropertyName("aro")]
	public string? Aro { get; set; }

	/// <summary>
	/// Access Request Object foreign key.
	/// </summary>
	[JsonPropertyName("aro_foreign_key")]
	public string? AroForeignKey { get; set; }

	/// <summary>
	/// Permission type.
	/// </summary>
	[JsonPropertyName("type")]
	public int? Type { get; set; }
}
