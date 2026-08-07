namespace Passbolt.Cli.Commands.Audit;

/// <summary>Settings for the resource-ownership audit.</summary>
public sealed class OwnershipAuditSettings : ConnectionSettings
{
	[CommandOption("--min-owners <N>")]
	[Description("Flag resources with fewer than N owner principals (default 2).")]
	public int MinOwners { get; set; } = 2;

	[CommandOption("--ignore-group-owner")]
	[Description("Do not flag resources that lack a group owner.")]
	public bool IgnoreGroupOwner { get; set; }

	[CommandOption("--all")]
	[Description("Show every resource, not just non-compliant ones.")]
	public bool All { get; set; }
}
