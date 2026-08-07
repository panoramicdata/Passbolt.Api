namespace Passbolt.Cli.Infrastructure;

/// <summary>Interactive confirmation for destructive actions.</summary>
public static class Confirmations
{
	/// <summary>
	/// Returns true if the user confirms. In JSON or non-interactive contexts there is no way to
	/// ask, so this returns false — the caller must pass --yes to proceed unattended.
	/// </summary>
	public static bool Confirm(string question, bool jsonMode)
	{
		if (jsonMode || !AnsiConsole.Profile.Capabilities.Interactive)
		{
			Output.Info("Refusing a destructive action without confirmation — pass --yes to proceed non-interactively.");
			return false;
		}

		return AnsiConsole.Confirm($"[yellow]{Markup.Escape(question)}[/]", defaultValue: false);
	}
}
