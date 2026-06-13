namespace Passbolt.Api.Requests;

/// <summary>
/// Request payload for updating a Passbolt resource.
/// </summary>
public sealed class UpdateResourceRequest
{
	/// <summary>
	/// Resource name.
	/// </summary>
	[JsonPropertyName("name")]
	public string? Name { get; set; }

	/// <summary>
	/// Resource URI.
	/// </summary>
	[JsonPropertyName("uri")]
	public string? Uri { get; set; }

	/// <summary>
	/// Resource username.
	/// </summary>
	[JsonPropertyName("username")]
	public string? Username { get; set; }

	/// <summary>
	/// Resource secret/password.
	/// </summary>
	[JsonPropertyName("secret")]
	public string? Secret { get; set; }

	/// <summary>
	/// Resource description.
	/// </summary>
	[JsonPropertyName("description")]
	public string? Description { get; set; }
}
