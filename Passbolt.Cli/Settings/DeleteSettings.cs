namespace Passbolt.Cli.Settings;

/// <summary>Id settings plus a confirmation-skip flag for destructive delete commands.</summary>
public class DeleteSettings : IdSettings
{
	[CommandOption("-y|--yes")]
	[Description("Skip the interactive confirmation prompt.")]
	public bool Yes { get; set; }
}
