namespace Passbolt.Cli.Commands.Groups;

/// <summary>Deletes a group by id (destructive; prompts unless --yes).</summary>
public sealed class DeleteGroupCommand : AsyncCommand<DeleteSettings>
{
	public override async Task<int> ExecuteAsync(CommandContext context, DeleteSettings settings)
	{
		using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
		using var client = ClientFactory.Create(settings);

		if (!settings.Yes && !Confirmations.Confirm($"Delete group '{settings.Id}'?", settings.Json))
		{
			Output.Info("Aborted.");
			return 1;
		}

		var response = await client.Groups.DeleteAsync(settings.Id, cts.Token);
		if (!response.IsSuccessStatusCode)
		{
			throw new CliException($"Delete failed: HTTP {response.StatusCode} {response.ReasonPhrase}.");
		}

		if (settings.Json)
		{
			Output.Json(new { deleted = settings.Id });
			return 0;
		}

		Output.Info($"Deleted group {settings.Id}.");
		return 0;
	}
}
