namespace Passbolt.Cli.Commands.Users;

/// <summary>Settings for updating a user.</summary>
public sealed class UpdateUserSettings : IdSettings
{
	[CommandOption("--first-name <NAME>")]
	[Description("New first name.")]
	public string? FirstName { get; set; }

	[CommandOption("--last-name <NAME>")]
	[Description("New last name.")]
	public string? LastName { get; set; }

	[CommandOption("--role-id <ID>")]
	[Description("New role id.")]
	public string? RoleId { get; set; }

	public override ValidationResult Validate()
		=> FirstName is null && LastName is null && RoleId is null
			? ValidationResult.Error("Specify at least one of --first-name, --last-name or --role-id.")
			: ValidationResult.Success();
}

/// <summary>Updates a user's profile fields.</summary>
public sealed class UpdateUserCommand : AsyncCommand<UpdateUserSettings>
{
	public override async Task<int> ExecuteAsync(CommandContext context, UpdateUserSettings settings)
	{
		using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
		using var client = ClientFactory.Create(settings);

		var updated = (await client.Users.UpdateAsync(settings.Id,
			new Passbolt.Api.Requests.UpdateUserRequest
			{
				FirstName = settings.FirstName,
				LastName = settings.LastName,
				RoleId = settings.RoleId
			}, cts.Token)).Value;

		if (settings.Json)
		{
			Output.Json(updated);
			return 0;
		}

		Output.Info($"Updated user {updated.Id}.");
		return 0;
	}
}
