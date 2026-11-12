using Passbolt.Api.Cryptography;

namespace Passbolt.Api;

/// <summary>Secret construction and recipient resolution behind the resource operations.</summary>
public sealed partial class PassboltClient
{
	/// <summary>
	/// Builds a secret for every user who will have access once the share is applied. When any
	/// recipient is newly granted access, Passbolt requires a secret for the complete resulting
	/// accessor set (current accessors + added - removed), not just for the added recipients.
	/// </summary>
	private async Task<List<SecretRequest>> BuildSecretsForResultingAccessorsAsync(
		string resourceId,
		string payload,
		IEnumerable<string> addedUserIds,
		IEnumerable<ShareSimulationSecret>? removedChanges,
		CancellationToken cancellationToken)
	{
		var removedUserIds = ExtractUserIds(removedChanges);
		var currentAccessorIds = (await Users.GetWithAccessToResourceAsync(resourceId, 1, cancellationToken).ConfigureAwait(false))
			.Value.Select(u => u.Id).Where(id => !string.IsNullOrEmpty(id)).Select(id => id!);

		var finalUserIds = currentAccessorIds
			.Concat(addedUserIds)
			.Distinct()
			.Where(id => !removedUserIds.Contains(id))
			.ToHashSet(StringComparer.Ordinal);

		var allUsers = (await Users.GetAllWithGpgKeysAsync(1, cancellationToken).ConfigureAwait(false)).Value;
		var recipients = allUsers.Where(u => u.Id is not null && finalUserIds.Contains(u.Id)).ToList();
		return BuildRecipientSecrets(payload, recipients);
	}

	private async Task<string> BuildRotationPayloadAsync(
		string resourceId,
		string slug,
		string password,
		string? description,
		CancellationToken cancellationToken)
	{
		if (!string.Equals(slug, PasswordAndDescriptionSlug, StringComparison.Ordinal))
		{
			return BuildSecretPayload(slug, password, description);
		}

		// Preserve the existing description when none is supplied.
		if (description is null)
		{
			var currentSecret = (await Secrets.GetForResourceAsync(resourceId, cancellationToken).ConfigureAwait(false)).Value;
			var currentPayload = PassboltPgp.Decrypt(currentSecret.Data!, _options.PrivateKeyBlock, _options.Password);
			try
			{
				using var document = JsonDocument.Parse(currentPayload);
				if (document.RootElement.TryGetProperty("description", out var existing) && existing.ValueKind == JsonValueKind.String)
				{
					description = existing.GetString();
				}
			}
			catch (JsonException)
			{
				// Fall through with a null description if the existing payload is not the expected JSON.
			}
		}

		return BuildSecretPayload(slug, password, description);
	}

	private static List<string> ExtractUserIds(IEnumerable<ShareSimulationSecret>? changes)
		=> changes?
			.Select(c => c.EffectiveUserId)
			.Where(id => !string.IsNullOrEmpty(id))
			.Select(id => id!)
			.Distinct()
			.ToList() ?? [];

	private List<SecretRequest> BuildRecipientSecrets(string payload, IEnumerable<User> recipients)
	{
		var secrets = new List<SecretRequest>();
		foreach (var user in recipients)
		{
			var armoredKey = user.Gpgkey?.ArmoredKey;
			if (string.IsNullOrWhiteSpace(user.Id) || string.IsNullOrWhiteSpace(armoredKey))
			{
				throw new InvalidOperationException($"User {user.Id} has no usable public key for secret encryption.");
			}

			secrets.Add(new SecretRequest
			{
				UserId = user.Id,
				Data = EncryptFor(payload, [armoredKey])
			});
		}

		if (secrets.Count == 0)
		{
			throw new InvalidOperationException("No recipients were resolved for the secret.");
		}

		return secrets;
	}

	private string EncryptFor(string payload, IReadOnlyCollection<string> recipientPublicKeys)
		=> PassboltPgp.EncryptAndSign(payload, recipientPublicKeys, _options.PrivateKeyBlock, _options.Password);

	private static string BuildSecretPayload(string slug, string password, string? description)
		=> string.Equals(slug, PasswordAndDescriptionSlug, StringComparison.Ordinal)
			? JsonSerializer.Serialize(
				new Dictionary<string, string> { ["password"] = password, ["description"] = description ?? string.Empty },
				SecretPayloadJsonOptions)
			: password;

	private async Task<ResourceType> ResolveResourceTypeAsync(string slug, CancellationToken cancellationToken)
	{
		var resourceTypes = (await ResourceTypes.GetAllAsync(cancellationToken).ConfigureAwait(false)).Value;
		return resourceTypes.FirstOrDefault(rt => string.Equals(rt.Slug, slug, StringComparison.Ordinal))
			?? throw new InvalidOperationException($"The server does not offer the '{slug}' resource type.");
	}

	private async Task<string> ResolveResourceTypeSlugAsync(string? resourceTypeId, CancellationToken cancellationToken)
	{
		if (string.IsNullOrEmpty(resourceTypeId))
		{
			return PasswordAndDescriptionSlug;
		}

		var resourceTypes = (await ResourceTypes.GetAllAsync(cancellationToken).ConfigureAwait(false)).Value;
		var match = resourceTypes.FirstOrDefault(rt => string.Equals(rt.Id, resourceTypeId, StringComparison.Ordinal));
		return match?.Slug ?? throw new InvalidOperationException($"Unknown resource type id '{resourceTypeId}'.");
	}

	private async Task<string> GetOwnPublicKeyAsync(CancellationToken cancellationToken)
	{
		var me = (await Me.GetAsync(cancellationToken).ConfigureAwait(false)).Value;
		var users = (await Users.GetAllWithGpgKeysAsync(1, cancellationToken).ConfigureAwait(false)).Value;
		var self = users.FirstOrDefault(u => u.Id == me.Id);
		return self?.Gpgkey?.ArmoredKey
			?? throw new InvalidOperationException("Could not resolve the current user's public key from the server.");
	}

	private static readonly JsonSerializerOptions SecretPayloadJsonOptions = new()
	{
		TypeInfoResolver = new DefaultJsonTypeInfoResolver()
	};
}
