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

    /// <summary>
    /// Creation time as Passbolt's REST API v2 sends it (<c>created</c>), for servers that do not send <see cref="CreatedTimestamp"/>.
    /// </summary>
    [JsonPropertyName("created")]
    public DateTimeOffset? Created { get; set; }

    /// <summary>
    /// Modification time as Passbolt's REST API v2 sends it (<c>modified</c>), for servers that do not send <see cref="ModifiedTimestamp"/>.
    /// </summary>
    [JsonPropertyName("modified")]
    public DateTimeOffset? Modified { get; set; }
}
