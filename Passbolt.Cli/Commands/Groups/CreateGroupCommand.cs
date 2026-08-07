namespace Passbolt.Cli.Commands.Groups;

/// <summary>Settings for creating a group.</summary>
public sealed class CreateGroupSettings : ConnectionSettings
{
	[CommandOption("--name <NAME>")]
	[Description("Group name.")]
	public string Name { get; set; } = string.Empty;

	[CommandOption("--manager <USER_ID>")]
	[Description("User id of the initial group manager (Passbolt requires at least one).")]
	public string Manager { get; set; } = string.Empty;

	public override ValidationResult Validate()
		=> string.IsNullOrWhiteSpace(Name) || string.IsNullOrWhiteSpace(Manager)
			? ValidationResult.Error("--name and --manager (initial manager user id) are required.")
			: ValidationResult.Success();
}

/// <summary>Creates a group with an initial manager.</summary>
public sealed class CreateGroupCommand : AsyncCommand<CreateGroupSettings>
{
	public override async Task<int> ExecuteAsync(CommandContext context, CreateGroupSettings settings)
	{
		using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
		using var client = ClientFactory.Create(settings);

		var created = (await client.Groups.CreateAsync(
			new Passbolt.Api.Requests.CreateGroupRequest
			{
				Name = settings.Name,
				GroupUsers = [new Passbolt.Api.Requests.GroupUserMembershipRequest { UserId = settings.Manager, IsAdmin = true }]
			}, cts.Token)).Value;

		if (settings.Json)
		{
			Output.Json(created);
			return 0;
		}

		Output.Info($"Created group {created.Id} ({created.Name}).");
		return 0;
	}
}
