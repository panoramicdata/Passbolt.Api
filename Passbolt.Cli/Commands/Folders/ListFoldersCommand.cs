namespace Passbolt.Cli.Commands.Folders;

/// <summary>Lists all folders.</summary>
public sealed class ListFoldersCommand : AsyncCommand<ConnectionSettings>
{
	public override Task<int> ExecuteAsync(CommandContext context, ConnectionSettings settings, CancellationToken cancellationToken)
		=> CommandRunner.RunAsync(settings, cancellationToken, async (client, token) => (await client.Folders.ListFoldersAsync(token)).Value, WriteFoldersTable);

	private static void WriteFoldersTable(IReadOnlyList<Folder> folders)
	{
		var table = Output.Grid("Id", "Name", "Personal", "Parent");
		foreach (var folder in folders)
		{
			table.AddRow(
				Output.Cell(folder.Id),
				Output.Cell(folder.Name),
				folder.Personal ? "yes" : "no",
				Output.Cell(folder.FolderParentId));
		}

		Output.Table(table);
		Output.Info($"{folders.Count} folder(s).");
	}
}
