namespace Passbolt.Cli.Commands.Users;

/// <summary>Deletes a user by id (destructive; prompts unless --yes).</summary>
public sealed class DeleteUserCommand : AsyncCommand<DeleteSettings>
{
	public override async Task<int> ExecuteAsync(CommandContext context, DeleteSettings settings)
	{
		using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
		using var client = ClientFactory.Create(settings);

		if (!settings.Yes && !Confirmations.Confirm($"Delete user '{settings.Id}'?", settings.Json))
		{
			Output.Info("Aborted.");
			return 1;
		}

		var response = await client.Users.DeleteAsync(settings.Id, cts.Token);
		if (!response.IsSuccessStatusCode)
		{
			throw new CliException($"Delete failed: HTTP {response.StatusCode} {response.ReasonPhrase}.");
		}

		if (settings.Json)
		{
			Output.Json(new { deleted = settings.Id });
			return 0;
		}

		Output.Info($"Deleted user {settings.Id}.");
		return 0;
	}
}
