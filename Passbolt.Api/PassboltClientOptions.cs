namespace Passbolt.Api;

/// <summary>
/// Configuration options for <see cref="PassboltClient"/>.
/// </summary>
public sealed class PassboltClientOptions
{
	/// <summary>
	/// Passbolt server base URL.
	/// </summary>
	public required Uri Uri { get; set; }

	/// <summary>
	/// Username
	/// </summary>
	public required string Username { get; set; }

	/// <summary>
	/// Password
	/// </summary>
	public required string Password { get; set; }

	/// <summary>
	/// PGP private key block for the user. This is used to sign requests to the Passbolt API.
	/// </summary>
	public required string PrivateKeyBlock { get; set; }

	/// <summary>
	/// Logger used for HTTP diagnostics. Defaults to a no-op logger.
	/// </summary>
	public ILogger Logger { get; set; } = NullLogger<PassboltClient>.Instance;

	/// <summary>
	/// Request timeout.
	/// </summary>
	public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);

	/// <summary>
	/// Optional <see cref="HttpClient"/> instance to use for API calls. If not provided, a new instance will be created internally.
	/// </summary>
	public HttpClient? HttpClient { get; internal set; }
}
