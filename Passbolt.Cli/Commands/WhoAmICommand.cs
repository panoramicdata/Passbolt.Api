namespace Passbolt.Cli.Commands;

/// <summary>Shows the currently authenticated Passbolt user.</summary>
public sealed class WhoAmICommand : AsyncCommand<ConnectionSettings>
{
	public override Task<int> ExecuteAsync(CommandContext context, ConnectionSettings settings, CancellationToken cancellationToken)
		=> CommandRunner.RunAsync(settings, cancellationToken, async (client, token) => (await client.Me.GetAsync(token)).Value, RenderTable);

	private static void RenderTable(User me)
	{
		var table = Output.TableBase();
		table.AddRow("Id", Output.Cell(me.Id));
		table.AddRow("Username", Output.Cell(me.Username));
		table.AddRow("Name", Output.Cell($"{me.FirstName} {me.LastName}".Trim()));
		table.AddRow("Active", me.Active.ToString());
		Output.Table(table);
	}
}
