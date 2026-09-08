namespace Passbolt.Cli.Commands.Folders;

/// <summary>Settings for updating a folder.</summary>
public sealed class UpdateFolderSettings : IdSettings
{
	[CommandOption("--name <NAME>")]
	[Description("New folder name.")]
	public string? Name { get; set; }

	[CommandOption("--description <TEXT>")]
	[Description("New folder description.")]
	public string? Description { get; set; }

	public override ValidationResult Validate()
		=> Name is null && Description is null
			? ValidationResult.Error("Specify at least one of --name or --description.")
			: ValidationResult.Success();
}

/// <summary>Updates a folder's name/description.</summary>
public sealed class UpdateFolderCommand : AsyncCommand<UpdateFolderSettings>
{
	protected override async Task<int> ExecuteAsync(CommandContext context, UpdateFolderSettings settings, CancellationToken cancellationToken)
	{
		using var client = await ClientFactory.CreateAsync(settings, cancellationToken);
		using var cts = CommandCancellation.WithTimeout(cancellationToken, TimeSpan.FromSeconds(30));

		var updated = (await client.Folders.UpdateFolderAsync(settings.Id,
			new Passbolt.Api.Requests.UpdateFolderRequest { Name = settings.Name, Description = settings.Description }, cts.Token)).Value;

		if (settings.Json)
		{
			Output.Json(updated);
			return 0;
		}

		Output.Info($"Updated folder {updated.Id}.");
		return 0;
	}
}
