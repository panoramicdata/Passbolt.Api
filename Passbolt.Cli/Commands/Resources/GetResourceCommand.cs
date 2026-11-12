namespace Passbolt.Cli.Commands.Resources;

/// <summary>Gets a single resource by id.</summary>
public sealed class GetResourceCommand : AsyncCommand<IdSettings>
{
	public override Task<int> ExecuteAsync(CommandContext context, IdSettings settings, CancellationToken cancellationToken)
		=> CommandRunner.RunAsync(settings, cancellationToken, async (client, token) => (await client.Resources.GetAsync(settings.Id, token)).Value, RenderTable);

	private static void RenderTable(Resource resource)
	{
		var table = Output.TableBase();
		table.AddRow("Id", Output.Cell(resource.Id));
		table.AddRow("Name", Output.Cell(resource.Name));
		table.AddRow("Username", Output.Cell(resource.Username));
		table.AddRow("URI", Output.Cell(resource.Uri));
		table.AddRow("Description", Output.Cell(resource.Description));
		table.AddRow("Folder parent", Output.Cell(resource.FolderParentId));
		Output.Table(table);
	}
}
