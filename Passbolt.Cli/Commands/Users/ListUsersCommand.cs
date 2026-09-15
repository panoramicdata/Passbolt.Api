namespace Passbolt.Cli.Commands.Users;

/// <summary>Lists all users.</summary>
public sealed class ListUsersCommand : AsyncCommand<ConnectionSettings>
{
	private const int TimeoutSeconds = 30;

	protected override async Task<int> ExecuteAsync(CommandContext context, ConnectionSettings settings, CancellationToken cancellationToken)
	{
		using var client = await ClientFactory.CreateAsync(settings, cancellationToken);
		using var cts = CommandCancellation.WithTimeout(cancellationToken, TimeSpan.FromSeconds(TimeoutSeconds));

		var users = (await client.Users.GetAllAsync(cts.Token)).Value;

		if (settings.Json)
		{
			Output.Json(users);
		}
		else
		{
			WriteUsersTable(users);
		}

		return 0;
	}

	private static void WriteUsersTable(IReadOnlyList<User> users)
	{
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
	}
}
