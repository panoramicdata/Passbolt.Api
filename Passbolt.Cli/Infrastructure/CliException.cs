namespace Passbolt.Cli.Infrastructure;

/// <summary>
/// A user-facing CLI error whose message is safe to print without a stack trace.
/// </summary>
public sealed class CliException : Exception
{
	public CliException(string message) : base(message)
	{
	}

	public CliException(string message, Exception innerException) : base(message, innerException)
	{
	}
}
