namespace Passbolt.Cli.Commands.Resources;

/// <summary>Lists all resources visible to the authenticated user.</summary>
public sealed class ListResourcesCommand : AsyncCommand<ConnectionSettings>
{
	public override async Task<int> ExecuteAsync(CommandContext context, ConnectionSettings settings)
	{
		using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
		using var client = ClientFactory.Create(settings);

		var resources = (await client.Resources.GetAllAsync(cts.Token)).Value;

		if (settings.Json)
		{
			Output.Json(resources);
			return 0;
		}

		var table = new Table().Border(TableBorder.Rounded);
		table.AddColumn("Id");
		table.AddColumn("Name");
		table.AddColumn("Username");
		table.AddColumn("URI");
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
		return 0;
	}
}
