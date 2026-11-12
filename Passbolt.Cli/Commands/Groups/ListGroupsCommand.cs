namespace Passbolt.Cli.Commands.Groups;

/// <summary>Lists all groups.</summary>
public sealed class ListGroupsCommand : AsyncCommand<ConnectionSettings>
{
	public override Task<int> ExecuteAsync(CommandContext context, ConnectionSettings settings, CancellationToken cancellationToken)
		=> CommandRunner.RunAsync(settings, cancellationToken, async (client, token) => (await client.Groups.ListAsync(token)).Value, WriteGroupsTable);

	private static void WriteGroupsTable(IReadOnlyList<Group> groups)
	{
		var table = Output.Grid("Id", "Name", "Users");
		foreach (var group in groups)
		{
			table.AddRow(
				Output.Cell(group.Id),
				Output.Cell(group.Name),
				group.UserCount.ToString());
		}

		Output.Table(table);
		Output.Info($"{groups.Count} group(s).");
	}
}
