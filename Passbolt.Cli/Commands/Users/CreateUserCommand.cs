namespace Passbolt.Cli.Commands.Users;

/// <summary>Settings for creating a user.</summary>
public sealed class CreateUserSettings : ConnectionSettings
{
	[CommandOption("--email <EMAIL>")]
	[Description("Email/username for the new user.")]
	public string Email { get; set; } = string.Empty;

	[CommandOption("--first-name <NAME>")]
	[Description("First name.")]
	public string FirstName { get; set; } = string.Empty;

	[CommandOption("--last-name <NAME>")]
	[Description("Last name.")]
	public string LastName { get; set; } = string.Empty;

	[CommandOption("--role-id <ID>")]
	[Description("Optional role id (see 'role list'); defaults to the server's default user role.")]
	public string? RoleId { get; set; }

	public override ValidationResult Validate()
		=> string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(FirstName) || string.IsNullOrWhiteSpace(LastName)
			? ValidationResult.Error("--email, --first-name and --last-name are required.")
			: ValidationResult.Success();
}

/// <summary>Creates (invites) a user. The user completes setup via the emailed invitation.</summary>
public sealed class CreateUserCommand : AsyncCommand<CreateUserSettings>
{
	public override async Task<int> ExecuteAsync(CommandContext context, CreateUserSettings settings)
	{
		using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
		using var client = ClientFactory.Create(settings);

		var created = (await client.Users.CreateAsync(
			new Passbolt.Api.Requests.CreateUserRequest
			{
				Username = settings.Email,
				FirstName = settings.FirstName,
				LastName = settings.LastName,
				RoleId = settings.RoleId
			}, cts.Token)).Value;

		if (settings.Json)
		{
			Output.Json(created);
			return 0;
		}

		Output.Info($"Created user {created.Id} ({created.Username}).");
		return 0;
	}
}
