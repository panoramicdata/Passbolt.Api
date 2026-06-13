namespace Passbolt.Api.Data;

/// <summary>
/// Represents a Passbolt group.
/// </summary>
public sealed class Group : NamedIdentifiedItem
{
    /// <summary>
    /// Group description.
    /// </summary>
	[JsonPropertyName("description")]
	public string? Description { get; set; }

    /// <summary>
    /// List of users in the group.
    /// </summary>
	[JsonPropertyName("users")]
	public IReadOnlyList<GroupUserMembership>? Users { get; set; }

    /// <summary>
    /// Indicates whether the group is deleted.
    /// </summary>
	[JsonPropertyName("deleted")]
	public bool Deleted { get; set; }

    /// <summary>
    /// Number of users in the group.
    /// </summary>
	[JsonPropertyName("user_count")]
	public int UserCount { get; set; }
}
