namespace Passbolt.Cli.Commands.Permissions;

/// <summary>Settings for listing the permissions on a resource.</summary>
public sealed class PermissionListSettings : ConnectionSettings
{
	[CommandOption("-r|--resource <ID>")]
	[Description("List permissions for this resource id.")]
	public string? ResourceId { get; set; }

	public override ValidationResult Validate()
		=> string.IsNullOrWhiteSpace(ResourceId)
			? ValidationResult.Error("Specify --resource.")
			: ValidationResult.Success();
}
