namespace Passbolt.Cli.Commands.Users;

/// <summary>Lists all users.</summary>
public sealed class ListUsersCommand : AsyncCommand<ConnectionSettings>
{
	public override async Task<int> ExecuteAsync(CommandContext context, ConnectionSettings settings)
	{
		using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
		using var client = ClientFactory.Create(settings);

		var users = (await client.Users.GetAllAsync(cts.Token)).Value;

		if (settings.Json)
		{
			Output.Json(users);
			return 0;
		}

		var table = new Table().Border(TableBorder.Rounded);
		table.AddColumn("Id");
		table.AddColumn("Username");
		table.AddColumn("Name");
		table.AddColumn("Active");
		foreach (var user in users)
		{
			table.AddRow(
				Output.Cell(user.Id),
				Output.Cell(user.Username),
				Output.Cell($"{user.FirstName} {user.LastName}".Trim()),
				user.Active ? "yes" : "no");
		}

		Output.Table(table);
		Output.Info($"{users.Count} user(s).");
		return 0;
	}
}
