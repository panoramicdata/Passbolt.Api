namespace Passbolt.Cli.Infrastructure;

/// <summary>Maps Passbolt permission type codes to human-readable names.</summary>
public static class PermissionLevel
{
	/// <summary>Passbolt "owner" permission type code.</summary>
	public static int Owner => 15;

	/// <summary>Renders a permission type code as a label (1=read, 7=update, 15=owner).</summary>
	public static string Describe(int? type) => type switch
	{
		1 => "read",
		7 => "update",
		15 => "owner",
		null => "(none)",
		_ => $"type {type}"
	};
}
