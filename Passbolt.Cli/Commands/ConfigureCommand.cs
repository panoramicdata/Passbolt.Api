namespace Passbolt.Cli.Commands;

/// <summary>Writes the non-secret CLI config (server, username, private-key file). Never stores the passphrase.</summary>
public sealed class ConfigureCommand : Command<ConnectionSettings>
{
	public override int Execute(CommandContext context, ConnectionSettings settings)
	{
		var (server, username, privateKeyFile) = ResolveWithPrompts(settings);

		if (string.IsNullOrWhiteSpace(server) || string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(privateKeyFile))
		{
			throw new CliException("configure needs --server, --username and --private-key-file (or run interactively).");
		}

		var config = new PassboltCliConfig
		{
			Server = server,
			Username = username,
			PrivateKeyFile = privateKeyFile
		};
		var savedPath = config.Save(settings.ConfigPath);

		if (settings.Json)
		{
			Output.Json(new { saved = savedPath, config });
			return 0;
		}

		Output.Info($"Saved configuration to {savedPath}");
		Output.Info("The private-key passphrase is never stored — supply it via --password, PASSBOLT_PASSWORD, or the prompt.");
		return 0;
	}

	/// <summary>
	/// Resolves the connection triple, prompting for whatever is still missing. Prompting is skipped
	/// in JSON/non-interactive mode, where an unattended run must fail rather than block on input.
	/// </summary>
	private static (string? Server, string? Username, string? PrivateKeyFile) ResolveWithPrompts(ConnectionSettings settings)
	{
		var (server, username, privateKeyFile) = ClientFactory.Resolve(settings);

		if (AnsiConsole.Profile.Capabilities.Interactive && !settings.Json)
		{
			server ??= AnsiConsole.Ask<string>("Server [green]URL[/]:");
			username ??= AnsiConsole.Ask<string>("[green]Username[/] (email):");
			privateKeyFile ??= AnsiConsole.Ask("Private-key [green]file path[/]:", PassboltCliConfig.ConventionalPrivateKeyPath);
		}

		return (server, username, privateKeyFile);
	}
}
