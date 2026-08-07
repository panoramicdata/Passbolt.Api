namespace Passbolt.Cli.Commands.Groups;

/// <summary>Gets a single group by id, including its members.</summary>
public sealed class GetGroupCommand : AsyncCommand<IdSettings>
{
	public override async Task<int> ExecuteAsync(CommandContext context, IdSettings settings)
	{
		using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
		using var client = ClientFactory.Create(settings);

		var group = (await client.Groups.GetAsync(settings.Id, cts.Token)).Value;

		if (settings.Json)
		{
			Output.Json(group);
			return 0;
		}

		var table = new Table().Border(TableBorder.Rounded);
		table.AddColumn("Field");
		table.AddColumn("Value");
		table.AddRow("Id", Output.Cell(group.Id));
		table.AddRow("Name", Output.Cell(group.Name));
		table.AddRow("Description", Output.Cell(group.Description));
		table.AddRow("User count", group.UserCount.ToString());
		Output.Table(table);
		return 0;
	}
}
