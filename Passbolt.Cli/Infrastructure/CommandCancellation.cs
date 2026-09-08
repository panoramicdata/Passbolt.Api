namespace Passbolt.Cli.Infrastructure;

/// <summary>Cancellation helpers shared by the commands.</summary>
public static class CommandCancellation
{
	/// <summary>
	/// Combines a command's own deadline with the token the CLI framework supplies for Ctrl+C, so
	/// the command stops both when it overruns and when the user interrupts it. Disposing the
	/// returned source detaches it from <paramref name="cancellationToken"/>.
	/// </summary>
	/// <param name="cancellationToken">The framework-supplied token, signalled on Ctrl+C.</param>
	/// <param name="timeout">How long the command may run before it cancels itself.</param>
	public static CancellationTokenSource WithTimeout(CancellationToken cancellationToken, TimeSpan timeout)
	{
		var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
		cts.CancelAfter(timeout);
		return cts;
	}
}
