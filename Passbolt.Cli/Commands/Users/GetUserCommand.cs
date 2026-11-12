namespace Passbolt.Cli.Commands.Users;

/// <summary>Gets a single user by id.</summary>
public sealed class GetUserCommand : AsyncCommand<IdSettings>
{
	public override Task<int> ExecuteAsync(CommandContext context, IdSettings settings, CancellationToken cancellationToken)
		=> CommandRunner.RunAsync(settings, cancellationToken, async (client, token) => (await client.Users.GetAsync(settings.Id, token)).Value, RenderTable);

	private static void RenderTable(User user)
	{
		var table = Output.TableBase();
		table.AddRow("Id", Output.Cell(user.Id));
		table.AddRow("Username", Output.Cell(user.Username));
		table.AddRow("Name", Output.Cell($"{user.FirstName} {user.LastName}".Trim()));
		table.AddRow("Active", user.Active ? "yes" : "no");
		table.AddRow("Deleted", user.Deleted ? "yes" : "no");
		Output.Table(table);
	}
}
