namespace Passbolt.Cli.Infrastructure;

/// <summary>
/// Persisted, non-secret CLI configuration (server URL, username, private-key file path).
/// The private-key passphrase is NEVER stored here — it is supplied per-invocation via
/// --password, the PASSBOLT_PASSWORD environment variable, or an interactive prompt.
/// </summary>
public sealed class PassboltCliConfig
{
	[JsonPropertyName("server")]
	public string? Server { get; set; }

	[JsonPropertyName("username")]
	public string? Username { get; set; }

	[JsonPropertyName("privateKeyFile")]
	public string? PrivateKeyFile { get; set; }

	/// <summary>Default config path: %APPDATA%/PanoramicData/Passbolt.Cli/config.json (XDG-equivalent on other OSes).</summary>
	public static string DefaultPath => Path.Combine(
		Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
		"PanoramicData", "Passbolt.Cli", "config.json");

	/// <summary>The conventional Passbolt private-key location, used as a last resort.</summary>
	public static string ConventionalPrivateKeyPath => Path.Combine(
		Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".passbolt", "private.asc");

	private static readonly JsonSerializerOptions JsonOptions = new()
	{
		WriteIndented = true,
		TypeInfoResolver = new DefaultJsonTypeInfoResolver(),
		DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
	};

	/// <summary>Loads config from the given path (or the default), returning an empty config if none exists.</summary>
	public static PassboltCliConfig Load(string? path)
	{
		var file = string.IsNullOrWhiteSpace(path) ? DefaultPath : path;
		if (!File.Exists(file))
		{
			return new PassboltCliConfig();
		}

		try
		{
			return JsonSerializer.Deserialize<PassboltCliConfig>(File.ReadAllText(file), JsonOptions) ?? new PassboltCliConfig();
		}
		catch (JsonException)
		{
			return new PassboltCliConfig();
		}
	}

	/// <summary>Writes this config to the given path (or the default), creating the directory as needed.</summary>
	public string Save(string? path)
	{
		var file = string.IsNullOrWhiteSpace(path) ? DefaultPath : path;
		var dir = Path.GetDirectoryName(file);
		if (!string.IsNullOrEmpty(dir))
		{
			Directory.CreateDirectory(dir);
		}

		File.WriteAllText(file, JsonSerializer.Serialize(this, JsonOptions));
		return file;
	}
}
