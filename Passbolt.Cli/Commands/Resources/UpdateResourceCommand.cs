namespace Passbolt.Cli.Commands.Resources;

/// <summary>Settings for updating a resource's metadata (not its secret).</summary>
public sealed class UpdateResourceSettings : IdSettings
{
	[CommandOption("--name <NAME>")]
	[Description("New resource name.")]
	public string? Name { get; set; }

	[CommandOption("--uri <URI>")]
	[Description("New resource URI.")]
	public string? Uri { get; set; }

	[CommandOption("--resource-username <USERNAME>")]
	[Description("New resource username (the credential's username, not your login).")]
	public string? ResourceUsername { get; set; }

	[CommandOption("--description <TEXT>")]
	[Description("New description.")]
	public string? Description { get; set; }

	public override ValidationResult Validate()
		=> Name is null && Uri is null && ResourceUsername is null && Description is null
			? ValidationResult.Error("Specify at least one of --name, --uri, --resource-username or --description.")
			: ValidationResult.Success();
}

/// <summary>
/// Updates a resource's metadata. Rotating the secret/password is intentionally NOT supported here
/// yet — it requires PGP encryption of the new secret to every recipient's key (tracked in #30).
/// </summary>
public sealed class UpdateResourceCommand : AsyncCommand<UpdateResourceSettings>
{
	public override async Task<int> ExecuteAsync(CommandContext context, UpdateResourceSettings settings)
	{
		using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
		using var client = ClientFactory.Create(settings);

		var updated = (await client.Resources.UpdateAsync(settings.Id,
			new Passbolt.Api.Requests.UpdateResourceRequest
			{
				Name = settings.Name,
				Uri = settings.Uri,
				Username = settings.ResourceUsername,
				Description = settings.Description
			}, cts.Token)).Value;

		if (settings.Json)
		{
			Output.Json(updated);
			return 0;
		}

		Output.Info($"Updated resource {updated.Id}.");
		return 0;
	}
}
