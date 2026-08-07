namespace Passbolt.Cli.Infrastructure;

/// <summary>
/// Resolves connection settings from options, environment and config, and builds a <see cref="PassboltClient"/>.
/// </summary>
public static class ClientFactory
{
	/// <summary>
	/// Resolves the effective (server, username, private-key file) triple, without requiring a passphrase.
	/// Used by both client construction and the <c>configure</c> command.
	/// </summary>
	public static (string? Server, string? Username, string? PrivateKeyFile) Resolve(ConnectionSettings settings)
	{
		var config = PassboltCliConfig.Load(settings.ConfigPath);

		var server = FirstNonEmpty(
			settings.Server,
			Environment.GetEnvironmentVariable("PASSBOLT_SERVER"),
			config.Server);

		var username = FirstNonEmpty(
			settings.Username,
			Environment.GetEnvironmentVariable("PASSBOLT_USERNAME"),
			config.Username);

		var privateKeyFile = FirstNonEmpty(
			settings.PrivateKeyFile,
			Environment.GetEnvironmentVariable("PASSBOLT_PRIVATE_KEY_FILE"),
			config.PrivateKeyFile);

		if (string.IsNullOrWhiteSpace(privateKeyFile) && File.Exists(PassboltCliConfig.ConventionalPrivateKeyPath))
		{
			privateKeyFile = PassboltCliConfig.ConventionalPrivateKeyPath;
		}

		return (server, username, privateKeyFile);
	}

	/// <summary>
	/// Builds a connected <see cref="PassboltClient"/> from the resolved settings, prompting for the
	/// passphrase when it was not supplied and a console is attached. Throws <see cref="CliException"/>
	/// with an actionable message when required inputs are missing.
	/// </summary>
	public static PassboltClient Create(ConnectionSettings settings)
	{
		var (server, username, privateKeyFile) = Resolve(settings);

		if (string.IsNullOrWhiteSpace(server))
		{
			throw new CliException("No server URL. Pass --server, set PASSBOLT_SERVER, or run 'passbolt configure'.");
		}

		if (!Uri.TryCreate(server, UriKind.Absolute, out var serverUri))
		{
			throw new CliException($"Server URL is not a valid absolute URI: '{server}'.");
		}

		if (string.IsNullOrWhiteSpace(username))
		{
			throw new CliException("No username. Pass --username, set PASSBOLT_USERNAME, or run 'passbolt configure'.");
		}

		if (string.IsNullOrWhiteSpace(privateKeyFile))
		{
			throw new CliException("No private key. Pass --private-key-file, set PASSBOLT_PRIVATE_KEY_FILE, run 'passbolt configure', or place it at ~/.passbolt/private.asc.");
		}

		if (!File.Exists(privateKeyFile))
		{
			throw new CliException($"Private key file not found: '{privateKeyFile}'.");
		}

		var privateKeyBlock = File.ReadAllText(privateKeyFile);

		var password = FirstNonEmpty(settings.Password, Environment.GetEnvironmentVariable("PASSBOLT_PASSWORD"));
		if (string.IsNullOrEmpty(password))
		{
			if (!AnsiConsole.Profile.Capabilities.Interactive)
			{
				throw new CliException("No passphrase. Pass --password, set PASSBOLT_PASSWORD, or run interactively to be prompted.");
			}

			password = AnsiConsole.Prompt(new TextPrompt<string>("Private-key [green]passphrase[/]:").Secret());
		}

		return new PassboltClient(new PassboltClientOptions
		{
			Uri = serverUri,
			Username = username,
			Password = password,
			PrivateKeyBlock = privateKeyBlock
		});
	}

	private static string? FirstNonEmpty(params string?[] candidates)
	{
		foreach (var candidate in candidates)
		{
			if (!string.IsNullOrWhiteSpace(candidate))
			{
				return candidate;
			}
		}

		return null;
	}
}
