namespace Passbolt.Cli.Commands.Resources;

/// <summary>Lists all resources visible to the authenticated user.</summary>
public sealed class ListResourcesCommand : AsyncCommand<ConnectionSettings>
{
	public override Task<int> ExecuteAsync(CommandContext context, ConnectionSettings settings, CancellationToken cancellationToken)
		=> CommandRunner.RunAsync(settings, cancellationToken, async (client, token) => (await client.Resources.GetAllAsync(token)).Value, WriteResourcesTable);

	private static void WriteResourcesTable(IReadOnlyList<Resource> resources)
	{
		var table = Output.Grid("Id", "Name", "Username", "URI");
		foreach (var resource in resources)
		{
			table.AddRow(
				Output.Cell(resource.Id),
				Output.Cell(resource.Name),
				Output.Cell(resource.Username),
				Output.Cell(resource.Uri));
		}

		Output.Table(table);
		Output.Info($"{resources.Count} resource(s).");
	}
}
