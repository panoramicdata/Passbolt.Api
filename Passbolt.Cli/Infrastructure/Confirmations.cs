namespace Passbolt.Cli.Infrastructure;

/// <summary>Interactive confirmation for destructive actions.</summary>
public static class Confirmations
{
	/// <summary>
	/// Returns true if the user confirms. In JSON or non-interactive contexts there is no way to
	/// ask, so this returns false — the caller must pass --yes to proceed unattended.
	/// </summary>
	public static async Task<bool> ConfirmAsync(string question, bool jsonMode, CancellationToken cancellationToken)
	{
		if (jsonMode || !AnsiConsole.Profile.Capabilities.Interactive)
		{
			Output.Info("Refusing a destructive action without confirmation — pass --yes to proceed non-interactively.");
			return false;
		}

		return await AnsiConsole.ConfirmAsync($"[yellow]{Markup.Escape(question)}[/]", defaultValue: false, cancellationToken);
	}
}
