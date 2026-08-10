using Passbolt.Api.Requests;

namespace Passbolt.Cli.Commands.Resources;

/// <summary>Settings for sharing a resource (granting or revoking access, re-encrypting as needed).</summary>
public sealed class ShareResourceSettings : IdSettings
{
	[CommandOption("--grant-user <USER_ID>")]
	[Description("Grant a user access (repeatable).")]
	public string[] GrantUsers { get; set; } = [];

	[CommandOption("--grant-group <GROUP_ID>")]
	[Description("Grant a group access (repeatable).")]
	public string[] GrantGroups { get; set; } = [];

	[CommandOption("--permission <LEVEL>")]
	[Description("Permission level for grants: read, update or owner (default: read).")]
	public string Permission { get; set; } = "read";

	[CommandOption("--revoke <PERMISSION_ID>")]
	[Description("Revoke an existing permission by its id (repeatable).")]
	public string[] Revoke { get; set; } = [];

	public override ValidationResult Validate()
	{
		if (GrantUsers.Length == 0 && GrantGroups.Length == 0 && Revoke.Length == 0)
		{
			return ValidationResult.Error("Specify at least one of --grant-user, --grant-group or --revoke.");
		}

		return Permission is "read" or "update" or "owner"
			? ValidationResult.Success()
			: ValidationResult.Error("--permission must be read, update or owner.");
	}
}

/// <summary>
/// Applies permission changes to a resource. Newly granted recipients have the secret re-encrypted
/// to their key automatically.
/// </summary>
public sealed class ShareResourceCommand : AsyncCommand<ShareResourceSettings>
{
	public override async Task<int> ExecuteAsync(CommandContext context, ShareResourceSettings settings)
	{
		using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(120));
		using var client = ClientFactory.Create(settings);

		var type = settings.Permission switch
		{
			"update" => 7,
			"owner" => PermissionLevel.Owner,
			_ => 1
		};

		var permissions = new List<SharePermissionRequest>();
		foreach (var userId in settings.GrantUsers)
		{
			permissions.Add(NewGrant("User", userId, settings.Id, type));
		}

		foreach (var groupId in settings.GrantGroups)
		{
			permissions.Add(NewGrant("Group", groupId, settings.Id, type));
		}

		foreach (var permissionId in settings.Revoke)
		{
			permissions.Add(new SharePermissionRequest { Id = permissionId, Delete = true });
		}

		var shared = await client.ShareResourceAsync(settings.Id, permissions, cts.Token);

		if (settings.Json)
		{
			Output.Json(shared);
			return 0;
		}

		Output.Info($"Updated sharing for resource {shared.Id}.");
		return 0;
	}

	private static SharePermissionRequest NewGrant(string aro, string aroId, string resourceId, int type)
		=> new()
		{
			IsNew = true,
			Aro = aro,
			AroForeignKey = aroId,
			Aco = "Resource",
			AcoForeignKey = resourceId,
			Type = type
		};
}
