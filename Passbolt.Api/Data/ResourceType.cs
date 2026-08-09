namespace Passbolt.Api.Data;

/// <summary>
/// Represents a Passbolt resource type. The <see cref="Slug"/> determines the shape of the
/// (encrypted) secret payload — for example a bare password string versus a JSON object
/// containing a password and a description.
/// </summary>
public sealed class ResourceType : NamedIdentifiedItem
{
	/// <summary>
	/// The stable slug identifying the resource type (for example
	/// <c>password-string</c> or <c>password-and-description</c>).
	/// </summary>
	[JsonPropertyName("slug")]
	public string? Slug { get; set; }

	/// <summary>
	/// Human-readable description of the resource type.
	/// </summary>
	[JsonPropertyName("description")]
	public string? Description { get; set; }

	/// <summary>
	/// The JSON schema definition describing the secret and metadata shape for this type.
	/// </summary>
	[JsonPropertyName("definition")]
	public JsonElement? Definition { get; set; }
}
