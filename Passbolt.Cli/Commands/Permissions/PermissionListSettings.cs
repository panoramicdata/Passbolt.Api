namespace Passbolt.Cli.Commands.Permissions;

/// <summary>Settings for listing permissions, scoped to a resource or a user.</summary>
public sealed class PermissionListSettings : ConnectionSettings
{
	[CommandOption("-r|--resource <ID>")]
	[Description("List permissions for this resource id.")]
	public string? ResourceId { get; set; }

	[CommandOption("-U|--user <ID>")]
	[Description("List permissions for this user id.")]
	public string? UserId { get; set; }

	public override ValidationResult Validate()
	{
		if (string.IsNullOrWhiteSpace(ResourceId) == string.IsNullOrWhiteSpace(UserId))
		{
			return ValidationResult.Error("Specify exactly one of --resource or --user.");
		}

		return ValidationResult.Success();
	}
}
