namespace Passbolt.Api.Data;

/// <summary>
/// Represents a Passbolt user.
/// </summary>
public sealed class User : IdentifiedItem
{
    /// <summary>
    /// Username/email address.
    /// </summary>
    [JsonPropertyName("username")]
    public string? Username { get; set; }

    /// <summary>
    /// User first name.
    /// </summary>
    [JsonPropertyName("first_name")]
    public string? FirstName { get; set; }

    /// <summary>
    /// User last name.
    /// </summary>
    [JsonPropertyName("last_name")]
    public string? LastName { get; set; }

    /// <summary>
    /// User role payload.
    /// </summary>
    [JsonPropertyName("role")]
    public JsonElement? Role { get; set; }

    /// <summary>
    /// Indicates whether the user is active.
    /// </summary>
    [JsonPropertyName("active")]
    public bool Active { get; set; }

    /// <summary>
    /// Indicates whether the user is deleted.
    /// </summary>
    [JsonPropertyName("deleted")]
    public bool Deleted { get; set; }

    /// <summary>
    /// User disabled payload.
    /// </summary>
    [JsonPropertyName("disabled")]
    public JsonElement? Disabled { get; set; }

    /// <summary>
    /// User profile payload.
    /// </summary>
    [JsonPropertyName("profile")]
    public JsonElement? Profile { get; set; }
}