namespace Passbolt.Cli.Commands.Resources;

/// <summary>Deletes a resource by id (destructive; prompts unless --yes).</summary>
public sealed class DeleteResourceCommand : AsyncCommand<DeleteSettings>
{
	public override async Task<int> ExecuteAsync(CommandContext context, DeleteSettings settings)
	{
		using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
		using var client = ClientFactory.Create(settings);

		if (!settings.Yes && !Confirmations.Confirm($"Delete resource '{settings.Id}'?", settings.Json))
		{
			Output.Info("Aborted.");
			return 1;
		}

		var response = await client.Resources.DeleteAsync(settings.Id, cts.Token);
		if (!response.IsSuccessStatusCode)
		{
			throw new CliException($"Delete failed: HTTP {response.StatusCode} {response.ReasonPhrase}.");
		}

		if (settings.Json)
		{
			Output.Json(new { deleted = settings.Id });
			return 0;
		}

		Output.Info($"Deleted resource {settings.Id}.");
		return 0;
	}
}
