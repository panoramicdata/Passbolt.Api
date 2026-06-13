namespace Passbolt.Api;

/// <summary>
/// Constants used throughout the Passbolt API client.
/// </summary>
internal static class PassboltConstants
{
	/// <summary>
	/// The Passbolt session cookie name.
	/// </summary>
	public const string PassboltSessionCookieName = "passbolt_session";

	/// <summary>
	/// The CakePHP cookie name prefix.
	/// </summary>
	public const string CakephpPrefix = "CAKEPHP";

	/// <summary>
	/// The PHP session ID cookie name.
	/// </summary>
	public const string PhpSessionIdCookieName = "PHPSESSID";

	/// <summary>
	/// The CSRF token cookie name.
	/// </summary>
	public const string CsrfTokenCookieName = "csrfToken";

	/// <summary>
	/// The GPG Authentication header name for user authentication.
	/// </summary>
	public const string GpgAuthUserAuthTokenHeader = "X-GPGAuth-User-Auth-Token";

	/// <summary>
	/// The GPG Authentication header name for encrypted token verification.
	/// </summary>
	public const string GpgAuthUserIdHeader = "X-GPGAuth-User-Id";

	/// <summary>
	/// The GPG Authentication header name for signature.
	/// </summary>
	public const string GpgAuthSignatureHeader = "X-GPGAuth-Signature";

	/// <summary>
	/// The expected number of fields in a GPG auth token.
	/// </summary>
	public const int ExpectedTokenFieldCount = 4;

	/// <summary>
	/// The status code for GPG authentication required.
	/// </summary>
	public const int GpgAuthRequiredStatusCode = 401;

	/// <summary>
	/// The status code for authentication success.
	/// </summary>
	public const int AuthSuccessStatusCode = 200;

	/// <summary>
	/// The default API version.
	/// </summary>
	public const string DefaultApiVersion = "v2";

	/// <summary>
	/// The JSON file extension.
	/// </summary>
	public const string JsonFileExtension = ".json";
}
