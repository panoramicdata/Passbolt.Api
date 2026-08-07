namespace Passbolt.Cli.Commands.Groups;

/// <summary>Lists all groups.</summary>
public sealed class ListGroupsCommand : AsyncCommand<ConnectionSettings>
{
	public override async Task<int> ExecuteAsync(CommandContext context, ConnectionSettings settings)
	{
		using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
		using var client = ClientFactory.Create(settings);

		var groups = (await client.Groups.ListAsync(cts.Token)).Value;

		if (settings.Json)
		{
			Output.Json(groups);
			return 0;
		}

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
		return 0;
	}
}
