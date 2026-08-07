namespace Passbolt.Cli.Commands.Folders;

/// <summary>Deletes a folder by id (destructive; prompts unless --yes).</summary>
public sealed class DeleteFolderCommand : AsyncCommand<DeleteSettings>
{
	public override async Task<int> ExecuteAsync(CommandContext context, DeleteSettings settings)
	{
		using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
		using var client = ClientFactory.Create(settings);

		if (!settings.Yes && !Confirmations.Confirm($"Delete folder '{settings.Id}'?", settings.Json))
		{
			Output.Info("Aborted.");
			return 1;
		}

		var response = await client.Folders.DeleteFolderAsync(settings.Id, cts.Token);
		if (!response.IsSuccessStatusCode)
		{
			throw new CliException($"Delete failed: HTTP {response.StatusCode} {response.ReasonPhrase}.");
		}

		if (settings.Json)
		{
			Output.Json(new { deleted = settings.Id });
			return 0;
		}

		Output.Info($"Deleted folder {settings.Id}.");
		return 0;
	}
}
