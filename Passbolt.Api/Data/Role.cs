namespace Passbolt.Api.Data;

/// <summary>
/// Represents a user role in Passbolt.
/// </summary>
public sealed class Role : NamedIdentifiedItem
{
	/// <summary>
	/// Gets or sets the description of the role.
	/// </summary>
	[JsonPropertyName("description")]
	public string? Description { get; set; }
}
