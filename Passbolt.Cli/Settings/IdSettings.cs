namespace Passbolt.Cli.Settings;

/// <summary>Connection settings plus a required positional entity id (for get/delete commands).</summary>
public class IdSettings : ConnectionSettings
{
	[CommandArgument(0, "<ID>")]
	[Description("The entity identifier (GUID).")]
	public string Id { get; set; } = string.Empty;
}
