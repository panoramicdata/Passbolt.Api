namespace Passbolt.Cli.Commands.Permissions;

/// <summary>Lists the permissions (principals + access level) on a resource or held by a user.</summary>
public sealed class ListPermissionsCommand : AsyncCommand<PermissionListSettings>
{
	public override async Task<int> ExecuteAsync(CommandContext context, PermissionListSettings settings)
	{
		using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
		using var client = ClientFactory.Create(settings);

		var permissions = string.IsNullOrWhiteSpace(settings.ResourceId)
			? (await client.Permissions.GetByUserAsync(settings.UserId!, cts.Token)).Value
			: (await client.Permissions.GetByResourceAsync(settings.ResourceId!, cts.Token)).Value;

		if (settings.Json)
		{
			Output.Json(permissions);
			return 0;
		}

		var table = new Table().Border(TableBorder.Rounded);
		table.AddColumn("Principal type");
		table.AddColumn("Principal id");
		table.AddColumn("Access");
		foreach (var permission in permissions)
		{
			table.AddRow(
				Output.Cell(permission.Aro),
				Output.Cell(permission.AroForeignKey),
				Output.Cell(PermissionLevel.Describe(permission.Type)));
		}

		Output.Table(table);
		Output.Info($"{permissions.Count} permission(s).");
		return 0;
	}
}
