namespace Passbolt.Cli.Commands.Groups;

/// <summary>Lists all groups.</summary>
public sealed class ListGroupsCommand : AsyncCommand<ConnectionSettings>
{
	public override async Task<int> ExecuteAsync(CommandContext context, ConnectionSettings settings, CancellationToken cancellationToken)
	{
		using var client = await ClientFactory.CreateAsync(settings, cancellationToken);
		using var cts = CommandCancellation.WithTimeout(cancellationToken, TimeSpan.FromSeconds(30));

		var groups = (await client.Groups.ListAsync(cts.Token)).Value;

		if (settings.Json)
		{
			Output.Json(groups);
		}
		else
		{
			WriteGroupsTable(groups);
		}

		return 0;
	}

	private static void WriteGroupsTable(IReadOnlyList<Group> groups)
	{
		var table = new Table().Border(TableBorder.Rounded);
		table.AddColumn("Id");
		table.AddColumn("Name");
		table.AddColumn("Users");
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
