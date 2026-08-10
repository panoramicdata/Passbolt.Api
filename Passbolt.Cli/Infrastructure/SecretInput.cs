namespace Passbolt.Cli.Infrastructure;

/// <summary>
/// Resolves a secret value (a password to store) either from an explicit option or, when omitted,
/// from a masked interactive prompt. Secrets are never echoed. Passing the value on the command
/// line is supported for automation but discouraged (it can leak into shell history).
/// </summary>
public static class SecretInput
{
	/// <summary>
	/// Returns the supplied secret, or prompts for it (masked) when a console is attached.
	/// </summary>
	/// <param name="supplied">The value from a command-line option, if any.</param>
	/// <param name="label">The prompt label (for example "New password").</param>
	/// <returns>The resolved secret value.</returns>
	public static string Resolve(string? supplied, string label)
	{
		if (!string.IsNullOrEmpty(supplied))
		{
			return supplied;
		}

		if (!AnsiConsole.Profile.Capabilities.Interactive)
		{
			throw new CliException($"No secret. Pass --secret or run interactively to be prompted for the {label.ToLowerInvariant()}.");
		}

		return AnsiConsole.Prompt(new TextPrompt<string>($"{label}:").Secret());
	}
}
