namespace Passbolt.Cli.Commands.Roles;

/// <summary>Lists available roles (useful for the --role-id on 'user create').</summary>
public sealed class ListRolesCommand : AsyncCommand<ConnectionSettings>
{
	public override async Task<int> ExecuteAsync(CommandContext context, ConnectionSettings settings)
	{
		using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
		using var client = ClientFactory.Create(settings);

		var roles = (await client.Roles.GetAllAsync(cts.Token)).Value;

		if (settings.Json)
		{
			Output.Json(roles);
			return 0;
		}

		var table = new Table().Border(TableBorder.Rounded);
		table.AddColumn("Id");
		table.AddColumn("Name");
		table.AddColumn("Description");
		foreach (var role in roles)
		{
			table.AddRow(Output.Cell(role.Id), Output.Cell(role.Name), Output.Cell(role.Description));
		}

		Output.Table(table);
		Output.Info($"{roles.Count} role(s).");
		return 0;
	}
}
