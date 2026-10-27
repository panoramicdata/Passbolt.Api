namespace Passbolt.Api.Mfa;

/// <summary>
/// How a <see cref="PassboltClient"/> gets past a server that requires a second factor (TOTP).
/// </summary>
public sealed class PassboltMfaOptions
{
	/// <summary>
	/// Where the remembered-device cookie is kept. Without one, every login needs a code.
	/// </summary>
	public IPassboltMfaCookieStore? CookieStore { get; set; }

	/// <summary>
	/// Supplies a one-time code when the server asks for one and no valid remembered cookie exists.
	/// Return null when no code can be given (for example in an unattended run): the client then
	/// throws <see cref="PassboltMfaRequiredException"/> rather than waiting for one.
	/// </summary>
	public Func<CancellationToken, Task<string?>>? TotpProvider { get; set; }

	/// <summary>
	/// Whether a verified code also asks the server to remember this device. Defaults to true.
	/// </summary>
	public bool RememberDevice { get; set; } = true;
}