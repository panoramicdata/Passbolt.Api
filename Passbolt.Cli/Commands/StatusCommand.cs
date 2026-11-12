namespace Passbolt.Cli.Commands;

/// <summary>Shows the Passbolt server status and healthcheck.</summary>
public sealed class StatusCommand : AsyncCommand<ConnectionSettings>
{
	public override Task<int> ExecuteAsync(CommandContext context, ConnectionSettings settings, CancellationToken cancellationToken)
		=> CommandRunner.RunAsync(
			settings,
			cancellationToken,
			async (client, token) => (Status: await client.Status.GetStatusAsync(token), Healthcheck: await client.Status.GetHealthcheckAsync(token)),
			result =>
			{
				var table = Output.TableBase();
				table.AddRow("Status", Output.Cell(result.Status.Status));
				Output.Table(table);
			},
			result => new { status = result.Status.Status, healthcheck = result.Healthcheck.Value });
}
