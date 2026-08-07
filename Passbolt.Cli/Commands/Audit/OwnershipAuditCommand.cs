namespace Passbolt.Cli.Commands.Audit;

/// <summary>
/// Audits resource ownership across the whole visible estate: for each resource it resolves the
/// owner principals (permission type 15) and flags those with fewer than N owners and/or no group
/// owner. This is the capability go-passbolt-cli does not surface, and the motivating requirement
/// for PanoramicData.Skills#540 / OPS-63894.
/// </summary>
public sealed class OwnershipAuditCommand : AsyncCommand<OwnershipAuditSettings>
{
	private sealed record OwnerRow(
		string? ResourceId,
		string? ResourceName,
		int OwnerCount,
		bool HasGroupOwner,
		IReadOnlyList<string> Owners,
		IReadOnlyList<string> Reasons);

	public override async Task<int> ExecuteAsync(CommandContext context, OwnershipAuditSettings settings)
	{
		using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(5));
		using var client = ClientFactory.Create(settings);

		// Resolve principal names once so the report is readable.
		var users = (await client.Users.GetAllAsync(cts.Token)).Value;
		var groups = (await client.Groups.ListAsync(cts.Token)).Value;
		var userNames = users
			.Where(u => u.Id is not null)
			.ToDictionary(u => u.Id!, u => $"{u.FirstName} {u.LastName}".Trim() is { Length: > 0 } n ? $"{n} <{u.Username}>" : (u.Username ?? u.Id!));
		var groupNames = groups
			.Where(g => g.Id is not null)
			.ToDictionary(g => g.Id!, g => g.Name ?? g.Id!);

		var resources = (await client.Resources.GetAllAsync(cts.Token)).Value;

		var rows = new List<OwnerRow>();
		foreach (var resource in resources)
		{
			if (resource.Id is null)
			{
				continue;
			}

			var permissions = (await client.Permissions.GetByResourceAsync(resource.Id, cts.Token)).Value;
			var owners = permissions.Where(p => p.Type == PermissionLevel.Owner).ToList();
			var hasGroupOwner = owners.Any(p => string.Equals(p.Aro, "Group", StringComparison.OrdinalIgnoreCase));

			var ownerLabels = owners
				.Select(p => string.Equals(p.Aro, "Group", StringComparison.OrdinalIgnoreCase)
					? $"[group] {Resolve(groupNames, p.AroForeignKey)}"
					: $"[user] {Resolve(userNames, p.AroForeignKey)}")
				.ToList();

			var reasons = new List<string>();
			if (owners.Count < settings.MinOwners)
			{
				reasons.Add($"< {settings.MinOwners} owners");
			}

			if (!settings.IgnoreGroupOwner && !hasGroupOwner)
			{
				reasons.Add("no group owner");
			}

			rows.Add(new OwnerRow(resource.Id, resource.Name, owners.Count, hasGroupOwner, ownerLabels, reasons));
		}

		var reported = settings.All ? rows : rows.Where(r => r.Reasons.Count > 0).ToList();

		if (settings.Json)
		{
			Output.Json(reported.Select(r => new
			{
				resourceId = r.ResourceId,
				resourceName = r.ResourceName,
				ownerCount = r.OwnerCount,
				hasGroupOwner = r.HasGroupOwner,
				owners = r.Owners,
				compliant = r.Reasons.Count == 0,
				reasons = r.Reasons
			}));
			return reported.Any(r => r.Reasons.Count > 0) ? 2 : 0;
		}

		var table = new Table().Border(TableBorder.Rounded);
		table.AddColumn("Resource");
		table.AddColumn("Id");
		table.AddColumn("Owners");
		table.AddColumn("Group owner");
		table.AddColumn("Issues");
		foreach (var row in reported)
		{
			table.AddRow(
				Output.Cell(row.ResourceName),
				Output.Cell(row.ResourceId),
				$"{row.OwnerCount}",
				row.HasGroupOwner ? "[green]yes[/]" : "[red]no[/]",
				row.Reasons.Count == 0 ? "[green]OK[/]" : $"[red]{Markup.Escape(string.Join("; ", row.Reasons))}[/]");
		}

		Output.Table(table);

		var nonCompliant = rows.Count(r => r.Reasons.Count > 0);
		Output.Info($"{resources.Count} resource(s) audited; {nonCompliant} non-compliant (min owners {settings.MinOwners}{(settings.IgnoreGroupOwner ? "" : ", group owner required")}).");
		return nonCompliant > 0 ? 2 : 0;
	}

	private static string Resolve(IReadOnlyDictionary<string, string> map, string? id)
		=> id is not null && map.TryGetValue(id, out var name) ? name : (id ?? "(unknown)");
}
