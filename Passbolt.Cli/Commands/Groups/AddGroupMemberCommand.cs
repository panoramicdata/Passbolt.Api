namespace Passbolt.Cli.Commands.Groups;

/// <summary>Settings for adding a member to a group.</summary>
public sealed class AddGroupMemberSettings : IdSettings
{
	[CommandOption("--user <USER_ID>")]
	[Description("User id to add to the group.")]
	public string UserId { get; set; } = string.Empty;

	[CommandOption("--admin")]
	[Description("Add the user as a group manager.")]
	public bool Admin { get; set; }

	public override ValidationResult Validate()
		=> string.IsNullOrWhiteSpace(UserId)
			? ValidationResult.Error("--user (user id) is required.")
			: ValidationResult.Success();
}

/// <summary>Adds a member to a group (a PUT with the new membership delta).</summary>
public sealed class AddGroupMemberCommand : AsyncCommand<AddGroupMemberSettings>
{
	public override async Task<int> ExecuteAsync(CommandContext context, AddGroupMemberSettings settings)
	{
		using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
		using var client = ClientFactory.Create(settings);

		var updated = (await client.Groups.UpdateAsync(settings.Id,
			new Passbolt.Api.Requests.UpdateGroupRequest
			{
				GroupUsers = [new Passbolt.Api.Requests.GroupUserMembershipRequest { UserId = settings.UserId, IsAdmin = settings.Admin }]
			}, cts.Token)).Value;

		if (settings.Json)
		{
			Output.Json(updated);
			return 0;
		}

		Output.Info($"Added user {settings.UserId} to group {settings.Id}{(settings.Admin ? " as manager" : "")}.");
		return 0;
	}
}
