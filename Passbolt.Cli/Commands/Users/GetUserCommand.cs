namespace Passbolt.Cli.Commands.Users;

/// <summary>Gets a single user by id.</summary>
public sealed class GetUserCommand : AsyncCommand<IdSettings>
{
	public override async Task<int> ExecuteAsync(CommandContext context, IdSettings settings)
	{
		using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
		using var client = ClientFactory.Create(settings);

		var user = (await client.Users.GetAsync(settings.Id, cts.Token)).Value;

		if (settings.Json)
		{
			Output.Json(user);
			return 0;
		}

		var table = new Table().Border(TableBorder.Rounded);
		table.AddColumn("Field");
		table.AddColumn("Value");
		table.AddRow("Id", Output.Cell(user.Id));
		table.AddRow("Username", Output.Cell(user.Username));
		table.AddRow("Name", Output.Cell($"{user.FirstName} {user.LastName}".Trim()));
		table.AddRow("Active", user.Active ? "yes" : "no");
		table.AddRow("Deleted", user.Deleted ? "yes" : "no");
		Output.Table(table);
		return 0;
	}
}
