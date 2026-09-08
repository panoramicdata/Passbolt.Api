namespace Passbolt.Cli.Commands;

/// <summary>Shows the Passbolt server status and healthcheck.</summary>
public sealed class StatusCommand : AsyncCommand<ConnectionSettings>
{
	protected override async Task<int> ExecuteAsync(CommandContext context, ConnectionSettings settings, CancellationToken cancellationToken)
	{
		using var client = await ClientFactory.CreateAsync(settings, cancellationToken);
		using var cts = CommandCancellation.WithTimeout(cancellationToken, TimeSpan.FromSeconds(30));

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
