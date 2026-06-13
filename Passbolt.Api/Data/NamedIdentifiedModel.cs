namespace Passbolt.Api.Data;

/// <summary>
/// Base class for items with an identifier and a name.
/// </summary>
public abstract class NamedIdentifiedItem : IdentifiedItem
{
    /// <summary>
    /// Name.
    /// </summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }
}
