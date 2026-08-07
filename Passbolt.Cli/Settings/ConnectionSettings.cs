namespace Passbolt.Cli.Settings;

/// <summary>
/// Options shared by every command that talks to a Passbolt server. Values resolve in the order:
/// explicit option → environment variable → config file → (for the private key) the conventional
/// ~/.passbolt/private.asc path.
/// </summary>
public class ConnectionSettings : CommandSettings
{
	[CommandOption("-s|--server <URL>")]
	[Description("Passbolt server base URL (env: PASSBOLT_SERVER).")]
	public string? Server { get; set; }

	[CommandOption("-u|--username <EMAIL>")]
	[Description("Passbolt account username/email (env: PASSBOLT_USERNAME).")]
	public string? Username { get; set; }

	[CommandOption("-p|--password <PASSPHRASE>")]
	[Description("Private-key passphrase (env: PASSBOLT_PASSWORD). Prompted for if omitted and a TTY is available.")]
	public string? Password { get; set; }

	[CommandOption("-k|--private-key-file <PATH>")]
	[Description("Path to the ASCII-armored PGP private key (env: PASSBOLT_PRIVATE_KEY_FILE).")]
	public string? PrivateKeyFile { get; set; }

	[CommandOption("-c|--config <PATH>")]
	[Description("Path to a CLI config file (default: %APPDATA%/PanoramicData/Passbolt.Cli/config.json).")]
	public string? ConfigPath { get; set; }

	[CommandOption("-j|--json")]
	[Description("Emit machine-readable JSON on stdout instead of a table.")]
	public bool Json { get; set; }
}
