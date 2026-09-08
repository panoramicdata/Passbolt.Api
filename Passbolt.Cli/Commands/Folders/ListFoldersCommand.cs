namespace Passbolt.Cli.Commands.Folders;

/// <summary>Lists all folders.</summary>
public sealed class ListFoldersCommand : AsyncCommand<ConnectionSettings>
{
	protected override async Task<int> ExecuteAsync(CommandContext context, ConnectionSettings settings, CancellationToken cancellationToken)
	{
		using var client = await ClientFactory.CreateAsync(settings, cancellationToken);
		using var cts = CommandCancellation.WithTimeout(cancellationToken, TimeSpan.FromSeconds(30));

		var folders = (await client.Folders.ListFoldersAsync(cts.Token)).Value;

		if (settings.Json)
		{
			Output.Json(folders);
			return 0;
		}

		var table = new Table().Border(TableBorder.Rounded);
		table.AddColumn("Id");
		table.AddColumn("Name");
		table.AddColumn("Personal");
		table.AddColumn("Parent");
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
		return 0;
	}
}
