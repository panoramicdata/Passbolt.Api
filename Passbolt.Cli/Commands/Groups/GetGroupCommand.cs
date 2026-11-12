namespace Passbolt.Cli.Commands.Groups;

/// <summary>Gets a single group by id, including its members.</summary>
public sealed class GetGroupCommand : AsyncCommand<IdSettings>
{
	public override Task<int> ExecuteAsync(CommandContext context, IdSettings settings, CancellationToken cancellationToken)
		=> CommandRunner.RunAsync(settings, cancellationToken, async (client, token) => (await client.Groups.GetAsync(settings.Id, token)).Value, RenderTable);

	private static void RenderTable(Group group)
	{
		var table = Output.TableBase();
		table.AddRow("Id", Output.Cell(group.Id));
		table.AddRow("Name", Output.Cell(group.Name));
		table.AddRow("Description", Output.Cell(group.Description));
		table.AddRow("User count", group.UserCount.ToString());
		Output.Table(table);
	}
}
