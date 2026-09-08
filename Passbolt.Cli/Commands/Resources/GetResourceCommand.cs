namespace Passbolt.Cli.Commands.Resources;

/// <summary>Gets a single resource by id.</summary>
public sealed class GetResourceCommand : AsyncCommand<IdSettings>
{
	protected override async Task<int> ExecuteAsync(CommandContext context, IdSettings settings, CancellationToken cancellationToken)
	{
		using var client = await ClientFactory.CreateAsync(settings, cancellationToken);
		using var cts = CommandCancellation.WithTimeout(cancellationToken, TimeSpan.FromSeconds(30));

		var resource = (await client.Resources.GetAsync(settings.Id, cts.Token)).Value;

		if (settings.Json)
		{
			Output.Json(resource);
			return 0;
		}

		var table = new Table().Border(TableBorder.Rounded);
		table.AddColumn("Field");
		table.AddColumn("Value");
		table.AddRow("Id", Output.Cell(resource.Id));
		table.AddRow("Name", Output.Cell(resource.Name));
		table.AddRow("Username", Output.Cell(resource.Username));
		table.AddRow("URI", Output.Cell(resource.Uri));
		table.AddRow("Description", Output.Cell(resource.Description));
		table.AddRow("Folder parent", Output.Cell(resource.FolderParentId));
		Output.Table(table);
		return 0;
	}
}
