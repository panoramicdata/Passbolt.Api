namespace Passbolt.Cli.Commands.Folders;

/// <summary>Gets a single folder by id.</summary>
public sealed class GetFolderCommand : AsyncCommand<IdSettings>
{
	public override async Task<int> ExecuteAsync(CommandContext context, IdSettings settings)
	{
		using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
		using var client = ClientFactory.Create(settings);

		var folder = (await client.Folders.GetFolderAsync(settings.Id, cts.Token)).Value;

		if (settings.Json)
		{
			Output.Json(folder);
			return 0;
		}

		var table = new Table().Border(TableBorder.Rounded);
		table.AddColumn("Field");
		table.AddColumn("Value");
		table.AddRow("Id", Output.Cell(folder.Id));
		table.AddRow("Name", Output.Cell(folder.Name));
		table.AddRow("Personal", folder.Personal ? "yes" : "no");
		table.AddRow("Parent", Output.Cell(folder.FolderParentId));
		Output.Table(table);
		return 0;
	}
}
