namespace Passbolt.Cli.Commands.Roles;

/// <summary>Lists available roles (useful for the --role-id on 'user create').</summary>
public sealed class ListRolesCommand : AsyncCommand<ConnectionSettings>
{
	public override Task<int> ExecuteAsync(CommandContext context, ConnectionSettings settings, CancellationToken cancellationToken)
		=> CommandRunner.RunAsync(settings, cancellationToken, async (client, token) => (await client.Roles.GetAllAsync(token)).Value, WriteRolesTable);

	private static void WriteRolesTable(IReadOnlyList<Role> roles)
	{
		var table = Output.Grid("Id", "Name", "Description");
		foreach (var role in roles)
		{
			table.AddRow(Output.Cell(role.Id), Output.Cell(role.Name), Output.Cell(role.Description));
		}

		Output.Table(table);
		Output.Info($"{roles.Count} role(s).");
	}
}
