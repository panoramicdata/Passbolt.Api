namespace Passbolt.Cli.Commands;

/// <summary>
/// Shared implementation of the destructive <c>delete</c> commands. Subclasses supply only the
/// entity noun and the API call, so the confirmation prompt, failure handling and JSON/human
/// output stay identical for every entity type.
/// </summary>
/// <param name="entityName">
/// The entity noun as it appears in prompts and messages (for example <c>group</c>).
/// </param>
public abstract class DeleteEntityCommand(string entityName) : AsyncCommand<DeleteSettings>
{
	protected override async Task<int> ExecuteAsync(CommandContext context, DeleteSettings settings, CancellationToken cancellationToken)
	{
		using var client = await ClientFactory.CreateAsync(settings, cancellationToken);

		if (!settings.Yes && !await Confirmations.ConfirmAsync($"Delete {entityName} '{settings.Id}'?", settings.Json, cancellationToken))
		{
			Output.Info("Aborted.");
			return 1;
		}

		// Started after the prompt, so time spent confirming is not charged to the deadline.
		using var cts = CommandCancellation.WithTimeout(cancellationToken, TimeSpan.FromSeconds(30));

		var response = await DeleteAsync(client, settings.Id, cts.Token);
		if (!response.IsSuccessStatusCode)
		{
			throw new CliException($"Delete failed: HTTP {response.StatusCode} {response.ReasonPhrase}.");
		}

		if (settings.Json)
		{
			Output.Json(new { deleted = settings.Id });
			return 0;
		}

		Output.Info($"Deleted {entityName} {settings.Id}.");
		return 0;
	}

	/// <summary>Issues the entity-specific delete call.</summary>
	protected abstract Task<Refit.IApiResponse> DeleteAsync(
		PassboltClient client,
		string id,
		CancellationToken cancellationToken);
}
