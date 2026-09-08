namespace Passbolt.Cli.Commands.Folders;

/// <summary>Settings for creating a folder.</summary>
public sealed class CreateFolderSettings : ConnectionSettings
{
	[CommandOption("--name <NAME>")]
	[Description("Folder name.")]
	public string Name { get; set; } = string.Empty;

	[CommandOption("--parent <FOLDER_ID>")]
	[Description("Optional parent folder id.")]
	public string? Parent { get; set; }

	public override ValidationResult Validate()
		=> string.IsNullOrWhiteSpace(Name)
			? ValidationResult.Error("--name is required.")
			: ValidationResult.Success();
}

/// <summary>Creates a folder.</summary>
public sealed class CreateFolderCommand : AsyncCommand<CreateFolderSettings>
{
	protected override async Task<int> ExecuteAsync(CommandContext context, CreateFolderSettings settings, CancellationToken cancellationToken)
	{
		using var client = await ClientFactory.CreateAsync(settings, cancellationToken);
		using var cts = CommandCancellation.WithTimeout(cancellationToken, TimeSpan.FromSeconds(30));

		var created = (await client.Folders.CreateFolderAsync(
			new Passbolt.Api.Requests.CreateFolderRequest { Name = settings.Name, ParentFolderId = settings.Parent }, cts.Token)).Value;

		if (settings.Json)
		{
			Output.Json(created);
			return 0;
		}

		Output.Info($"Created folder {created.Id} ({created.Name}).");
		return 0;
	}
}
