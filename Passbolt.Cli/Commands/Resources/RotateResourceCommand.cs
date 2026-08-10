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
	public override async Task<int> ExecuteAsync(CommandContext context, RotateResourceSettings settings)
	{
		using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(120));
		using var client = ClientFactory.Create(settings);

		var secret = SecretInput.Resolve(settings.Secret, "New password");

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
