namespace Passbolt.Api.Data;

/// <summary>
/// Represents a user role in Passbolt.
/// </summary>
public sealed class Role : IdentifiedItem
{
	/// <summary>
	/// Gets or sets the name of the role.
	/// </summary>
	public required string Name { get; set; }

	/// <summary>
	/// Gets or sets the description of the role.
	/// </summary>
	public string? Description { get; set; }

	/// <summary>
	/// Gets or sets the creation timestamp.
	/// </summary>
	public required DateTime Created { get; set; }

	/// <summary>
	/// Gets or sets the last modification timestamp.
	/// </summary>
	public required DateTime Modified { get; set; }
}
