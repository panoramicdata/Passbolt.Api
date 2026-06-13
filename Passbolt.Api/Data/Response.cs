namespace Passbolt.Api.Data;

/// <summary>
/// Standard envelope returned by Passbolt API endpoints.
/// </summary>
/// <typeparam name="T">Body payload type.</typeparam>
public sealed class Response<T>
{
	/// <summary>
	/// Header metadata for the API response.
	/// </summary>
	[JsonPropertyName("header")]
	public Header? Header { get; set; }

	/// <summary>
	/// Body payload for the API response.
	/// </summary>
	[JsonPropertyName("body")]
	public required T Value { get; set; }
}
