namespace Passbolt.Api.Mfa;

/// <summary>
/// The server requires a one-time code and none could be supplied, or the code was refused.
/// </summary>
public sealed class PassboltMfaRequiredException : InvalidOperationException
{
	/// <summary>
	/// Creates the exception with a message that says what to do next.
	/// </summary>
	/// <param name="message">The message.</param>
	public PassboltMfaRequiredException(string message) : base(message)
	{
	}
}