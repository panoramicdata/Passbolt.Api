namespace Passbolt.Cli.Commands;

/// <summary>Shows the currently authenticated Passbolt user.</summary>
public sealed class WhoAmICommand : AsyncCommand<ConnectionSettings>
{
	public override async Task<int> ExecuteAsync(CommandContext context, ConnectionSettings settings)
	{
		using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
		using var client = ClientFactory.Create(settings);

		var me = (await client.Me.GetAsync(cts.Token)).Value;

		if (settings.Json)
		{
			Output.Json(me);
			return 0;
		}

		var table = new Table().Border(TableBorder.Rounded);
		table.AddColumn("Field");
		table.AddColumn("Value");
		table.AddRow("Id", Output.Cell(me.Id));
		table.AddRow("Username", Output.Cell(me.Username));
		table.AddRow("Name", Output.Cell($"{me.FirstName} {me.LastName}".Trim()));
		table.AddRow("Active", me.Active.ToString());
		Output.Table(table);
		return 0;
	}
}
