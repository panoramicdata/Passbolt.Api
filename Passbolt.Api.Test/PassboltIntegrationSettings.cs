namespace Passbolt.Api.Test;

/// <summary>
/// Integration-test settings for live Passbolt API execution.
/// </summary>
public sealed class PassboltIntegrationSettings
{
	/// <summary>
	/// Passbolt server URL.
	/// </summary>
	public string ServerUrl { get; set; } = "https://passbolt.panoramicdata.com";

	/// <summary>
	/// API version segment.
	/// </summary>
	public string ApiVersion { get; set; } = "v2";

	/// <summary>
	/// Bearer token for authenticated API operations.
	/// </summary>
	public string? AccessToken { get; set; }

	/// <summary>
	/// Optional username used for environments that require explicit principal context.
	/// </summary>
	public string? Username { get; set; }

	/// <summary>
	/// Optional password/passphrase used as an auth fallback when a bearer token is not provided.
	/// </summary>
	public string? Password { get; set; }

	/// <summary>
	/// Optional armored PGP private key block sourced from user secrets.
	/// </summary>
	public string? PrivateKeyBlock { get; set; }

	/// <summary>
	/// Known user identifier for get-user integration scenarios.
	/// </summary>
	public string? KnownUserId { get; set; }

	/// <summary>
	/// Known group identifier for get-group integration scenarios.
	/// </summary>
	public string? KnownGroupId { get; set; }

	/// <summary>
	/// Known resource identifier for get-resource integration scenarios.
	/// </summary>
	public string? KnownResourceId { get; set; }

	/// <summary>
	/// Resource name used for get-resource-by-name tests.
	/// </summary>
	public string? ResourceLookupName { get; set; }

	/// <summary>
	/// Resource URI used for get-resource-by-uri tests.
	/// </summary>
	public string? ResourceLookupUri { get; set; }

	/// <summary>
	/// Optional username for URI-based lookup narrowing.
	/// </summary>
	public string? ResourceLookupUsername { get; set; }

	/// <summary>
	/// When true, mutating integration tests are enabled.
	/// </summary>
	public bool RunMutatingIntegrationTests { get; set; }

	/// <summary>
	/// Indicates if authenticated integration tests can run.
	/// </summary>
	public bool IsAuthenticatedConfigured =>
		!string.IsNullOrWhiteSpace(Username)
		&& !string.IsNullOrWhiteSpace(Password)
		&& !string.IsNullOrWhiteSpace(PrivateKeyBlock);

	/// <summary>
	/// Creates a configured Passbolt client.
	/// </summary>
	public PassboltClient CreateClient(ILogger? logger = null) => new(new PassboltClientOptions
	{
		Uri = new Uri(ServerUrl),
		Username = Username!,
		Password = Password!,
		PrivateKeyBlock = PrivateKeyBlock!,
		Logger = logger ?? NullLogger<PassboltClient>.Instance
	});
}
