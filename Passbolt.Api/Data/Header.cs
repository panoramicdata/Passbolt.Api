namespace Passbolt.Api.Data;

/// <summary>
/// Header metadata returned by Passbolt.
/// </summary>
public sealed class Header
{
	/// <summary>
	/// Response status.
	/// </summary>
	[JsonPropertyName("status")]
	public string? Status { get; set; }

	/// <summary>
	/// Optional server message.
	/// </summary>
	[JsonPropertyName("message")]
	public string? Message { get; set; }

	/// <summary>
	/// Additional status fields not explicitly mapped.
	/// </summary>
	[JsonExtensionData]
	public Dictionary<string, JsonElement>? AdditionalProperties { get; set; }
}
