namespace Passbolt.Cli.Infrastructure;

/// <summary>
/// Rendering helpers. In JSON mode stdout carries only the JSON payload (so it pipes cleanly into
/// jq/ConvertFrom-Json); tables and status chatter go to stderr or are suppressed.
/// </summary>
public static class Output
{
	private static readonly JsonSerializerOptions JsonOptions = new()
	{
		WriteIndented = true,
		TypeInfoResolver = new DefaultJsonTypeInfoResolver(),
		DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
	};

	/// <summary>Serializes a value as indented JSON to stdout.</summary>
	public static void Json<T>(T value)
		=> Console.Out.WriteLine(JsonSerializer.Serialize(value, JsonOptions));

	/// <summary>Writes a Spectre table to stdout (human mode only).</summary>
	public static void Table(Table value)
		=> AnsiConsole.Write(value);

	/// <summary>Writes an informational line to stderr, so it never pollutes JSON stdout.</summary>
	public static void Info(string message)
		=> Console.Error.WriteLine(message);

	/// <summary>Escapes a possibly-null value for safe display in a Spectre table cell.</summary>
	public static string Cell(string? value)
		=> Markup.Escape(value ?? string.Empty);
}
