namespace Passbolt.Cli.Commands;

/// <summary>Shows the Passbolt server status and healthcheck.</summary>
public sealed class StatusCommand : AsyncCommand<ConnectionSettings>
{
	public override async Task<int> ExecuteAsync(CommandContext context, ConnectionSettings settings)
	{
		using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
		using var client = ClientFactory.Create(settings);

		var status = await client.Status.GetStatusAsync(cts.Token);
		var healthcheck = await client.Status.GetHealthcheckAsync(cts.Token);

		if (settings.Json)
		{
			Output.Json(new { status = status.Status, healthcheck = healthcheck.Value });
			return 0;
		}

		var table = new Table().Border(TableBorder.Rounded);
		table.AddColumn("Field");
		table.AddColumn("Value");
		table.AddRow("Status", Output.Cell(status.Status));
		Output.Table(table);
		return 0;
	}
}
