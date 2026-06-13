namespace Passbolt.Api.Data;

/// <summary>
/// Server status payload returned by Passbolt.
/// </summary>
public sealed class ServerStatus
{
	/// <summary>
	/// Server status value.
	/// </summary>
	[JsonPropertyName("status")]
	public string? Status { get; set; }
}
