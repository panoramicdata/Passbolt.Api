namespace Passbolt.Cli.Commands.Resources;

/// <summary>Settings for creating a resource (with client-side PGP encryption of the secret).</summary>
public sealed class CreateResourceSettings : ConnectionSettings
{
	[CommandOption("--name <NAME>")]
	[Description("Resource name (required).")]
	public string? Name { get; set; }

	[CommandOption("--resource-username <USERNAME>")]
	[Description("The credential's username (not your login).")]
	public string? ResourceUsername { get; set; }

	[CommandOption("--uri <URI>")]
	[Description("Resource URI.")]
	public string? Uri { get; set; }

	[CommandOption("--secret <VALUE>")]
	[Description("The password to store. Prompted for (masked) if omitted.")]
	public string? Secret { get; set; }

	[CommandOption("--description <TEXT>")]
	[Description("Optional description.")]
	public string? Description { get; set; }

	[CommandOption("--folder <ID>")]
	[Description("Optional parent folder id.")]
	public string? FolderId { get; set; }

	[CommandOption("--plaintext-description")]
	[Description("Use the password-string type (unencrypted description) instead of password-and-description.")]
	public bool PlaintextDescription { get; set; }

	public override ValidationResult Validate()
		=> string.IsNullOrWhiteSpace(Name)
			? ValidationResult.Error("--name is required.")
			: ValidationResult.Success();
}

/// <summary>
/// Creates a resource, encrypting the secret to the current user with their OpenPGP key.
/// </summary>
public sealed class CreateResourceCommand : AsyncCommand<CreateResourceSettings>
{
	protected override async Task<int> ExecuteAsync(CommandContext context, CreateResourceSettings settings, CancellationToken cancellationToken)
	{
		using var client = await ClientFactory.CreateAsync(settings, cancellationToken);

		var secret = await SecretInput.ResolveAsync(settings.Secret, "Password", cancellationToken);

		// Started after the prompt, so time spent typing the secret is not charged to the deadline.
		using var cts = CommandCancellation.WithTimeout(cancellationToken, TimeSpan.FromSeconds(60));

		var created = await client.CreateResourceAsync(
			settings.Name!,
			settings.ResourceUsername,
			settings.Uri,
			secret,
			settings.Description,
			settings.FolderId,
			encryptDescription: !settings.PlaintextDescription,
			cts.Token);

		if (settings.Json)
		{
			Output.Json(created);
			return 0;
		}

		Output.Info($"Created resource {created.Id} ('{created.Name}').");
		return 0;
	}
}
