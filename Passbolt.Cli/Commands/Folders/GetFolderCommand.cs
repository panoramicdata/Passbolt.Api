namespace Passbolt.Cli.Commands.Folders;

/// <summary>Gets a single folder by id.</summary>
public sealed class GetFolderCommand : AsyncCommand<IdSettings>
{
	public override Task<int> ExecuteAsync(CommandContext context, IdSettings settings, CancellationToken cancellationToken)
		=> CommandRunner.RunAsync(settings, cancellationToken, async (client, token) => (await client.Folders.GetFolderAsync(settings.Id, token)).Value, RenderTable);

	private static void RenderTable(Folder folder)
	{
		var table = Output.TableBase();
		table.AddRow("Id", Output.Cell(folder.Id));
		table.AddRow("Name", Output.Cell(folder.Name));
		table.AddRow("Personal", folder.Personal ? "yes" : "no");
		table.AddRow("Parent", Output.Cell(folder.FolderParentId));
		Output.Table(table);
	}
}
