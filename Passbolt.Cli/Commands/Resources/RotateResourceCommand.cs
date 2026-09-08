namespace Passbolt.Cli.Commands.Resources;

/// <summary>Settings for rotating a resource's secret (re-encrypted for all recipients).</summary>
public sealed class RotateResourceSettings : IdSettings
{
	[CommandOption("--secret <VALUE>")]
	[Description("The new password. Prompted for (masked) if omitted.")]
	public string? Secret { get; set; }

	[CommandOption("--description <TEXT>")]
	[Description("New description. For password-and-description resources, omitting this preserves the existing description.")]
	public string? Description { get; set; }
}

/// <summary>
/// Rotates a resource's secret, re-encrypting the new value for every user with access (groups are
/// expanded server-side).
/// </summary>
public sealed class RotateResourceCommand : AsyncCommand<RotateResourceSettings>
{
	protected override async Task<int> ExecuteAsync(CommandContext context, RotateResourceSettings settings, CancellationToken cancellationToken)
	{
		using var client = await ClientFactory.CreateAsync(settings, cancellationToken);

		var secret = await SecretInput.ResolveAsync(settings.Secret, "New password", cancellationToken);

		// Started after the prompt, so time spent typing the secret is not charged to the deadline.
		using var cts = CommandCancellation.WithTimeout(cancellationToken, TimeSpan.FromSeconds(120));

		var updated = await client.RotateResourceSecretAsync(settings.Id, secret, settings.Description, cts.Token);

		if (settings.Json)
		{
			Output.Json(updated);
			return 0;
		}

		Output.Info($"Rotated secret for resource {updated.Id}.");
		return 0;
	}
}
