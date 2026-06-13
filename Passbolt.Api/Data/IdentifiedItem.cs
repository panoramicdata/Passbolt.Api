namespace Passbolt.Api.Data;

/// <summary>
/// Base class for items with an identifier.
/// </summary>
public abstract class IdentifiedItem
{
    /// <summary>
    /// Identifier.
    /// </summary>
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    /// <summary>
    /// UTC creation timestamp.
    /// </summary>
    [JsonPropertyName("created_timestamp")]
    public DateTimeOffset? CreatedTimestamp { get; set; }

    /// <summary>
    /// UTC modification timestamp.
    /// </summary>
    [JsonPropertyName("modified_timestamp")]
    public DateTimeOffset? ModifiedTimestamp { get; set; }
}
