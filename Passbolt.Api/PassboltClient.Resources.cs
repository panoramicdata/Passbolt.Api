using Passbolt.Api.Cryptography;

namespace Passbolt.Api;

/// <summary>
/// High-level, PGP-aware resource operations. These helpers perform the client-side encryption
/// that Passbolt requires: a secret is encrypted (and signed) once per recipient, so creating,
/// rotating and sharing a secret each involve resolving recipients' public keys and producing an
/// armored OpenPGP message for every one of them.
/// </summary>
public sealed partial class PassboltClient
{
	/// <summary>The <c>password-string</c> resource type keeps the password only (description is plaintext).</summary>
	public static string PasswordStringSlug => "password-string";

	/// <summary>The <c>password-and-description</c> resource type encrypts a JSON password/description pair.</summary>
	public static string PasswordAndDescriptionSlug => "password-and-description";

	/// <summary>
	/// Creates a resource, encrypting the secret to the current user. When
	/// <paramref name="encryptDescription"/> is true the <c>password-and-description</c> type is
	/// used and the description is stored inside the encrypted secret; otherwise the
	/// <c>password-string</c> type is used and the description (if any) is stored as plaintext.
	/// </summary>
	/// <param name="name">Resource name.</param>
	/// <param name="username">Resource username (the credential's username).</param>
	/// <param name="uri">Resource URI.</param>
	/// <param name="password">The secret password.</param>
	/// <param name="description">Optional description.</param>
	/// <param name="folderParentId">Optional parent folder identifier.</param>
	/// <param name="encryptDescription">Whether to encrypt the description inside the secret.</param>
	/// <param name="cancellationToken">Cancellation token.</param>
	/// <returns>The created resource.</returns>
	public async Task<Resource> CreateResourceAsync(
		string name,
		string? username,
		string? uri,
		string password,
		string? description,
		string? folderParentId,
		bool encryptDescription,
		CancellationToken cancellationToken)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(name);
		ArgumentNullException.ThrowIfNull(password);

		var slug = encryptDescription ? PasswordAndDescriptionSlug : PasswordStringSlug;
		var resourceType = await ResolveResourceTypeAsync(slug, cancellationToken).ConfigureAwait(false);
		var payload = BuildSecretPayload(slug, password, description);

		var ownPublicKey = await GetOwnPublicKeyAsync(cancellationToken).ConfigureAwait(false);
		var encrypted = EncryptFor(payload, [ownPublicKey]);

		var request = new CreateResourceRequest
		{
			Name = name,
			Username = username,
			Uri = uri,
			Description = encryptDescription ? null : description,
			ResourceTypeId = resourceType.Id,
			ParentFolderId = folderParentId,
			Secrets = [new SecretRequest { Data = encrypted }]
		};

		var response = await Resources.CreateAsync(request, cancellationToken).ConfigureAwait(false);
		return response.Value;
	}

	/// <summary>
	/// Rotates a resource's secret, re-encrypting the new secret for every user who currently has
	/// access (group membership is expanded server-side). For a <c>password-and-description</c>
	/// resource, a null <paramref name="description"/> preserves the existing description.
	/// </summary>
	/// <param name="resourceId">The resource whose secret to rotate.</param>
	/// <param name="password">The new password.</param>
	/// <param name="description">The new description, or null to leave it unchanged.</param>
	/// <param name="cancellationToken">Cancellation token.</param>
	/// <returns>The updated resource.</returns>
	public async Task<Resource> RotateResourceSecretAsync(
		string resourceId,
		string password,
		string? description,
		CancellationToken cancellationToken)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(resourceId);
		ArgumentNullException.ThrowIfNull(password);

		var resource = (await Resources.GetAsync(resourceId, cancellationToken).ConfigureAwait(false)).Value;
		var slug = await ResolveResourceTypeSlugAsync(resource.ResourceTypeId, cancellationToken).ConfigureAwait(false);

		var payload = await BuildRotationPayloadAsync(resourceId, slug, password, description, cancellationToken).ConfigureAwait(false);

		var recipients = (await Users.GetWithAccessToResourceAsync(resourceId, 1, cancellationToken).ConfigureAwait(false)).Value;
		var secrets = BuildRecipientSecrets(payload, recipients);

		var request = new UpdateResourceRequest
		{
			Name = resource.Name,
			Username = resource.Username,
			Uri = resource.Uri,
			ResourceTypeId = resource.ResourceTypeId,
			Secrets = secrets
		};

		var response = await Resources.UpdateAsync(resourceId, request, cancellationToken).ConfigureAwait(false);
		return response.Value;
	}

	/// <summary>
	/// Applies permission changes to a resource, re-encrypting the secret for every recipient that
	/// newly gains access. The existing secret is read and decrypted with the current user's key,
	/// then re-encrypted for each added recipient.
	/// </summary>
	/// <param name="resourceId">The resource to share.</param>
	/// <param name="permissions">The permission changes (grants, updates, revocations).</param>
	/// <param name="cancellationToken">Cancellation token.</param>
	/// <returns>The shared resource.</returns>
	public async Task<Resource> ShareResourceAsync(
		string resourceId,
		IReadOnlyList<SharePermissionRequest> permissions,
		CancellationToken cancellationToken)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(resourceId);
		ArgumentNullException.ThrowIfNull(permissions);

		// Decrypt the current secret so it can be re-encrypted for newly added recipients.
		var currentSecret = (await Secrets.GetForResourceAsync(resourceId, cancellationToken).ConfigureAwait(false)).Value;
		var payload = PassboltPgp.Decrypt(currentSecret.Data!, _options.PrivateKeyBlock, _options.Password);

		var shareRequest = new ShareResourceRequest { Permissions = [.. permissions] };

		// Ask the server which recipients would gain or lose access (expands groups authoritatively).
		var simulation = (await Resources.SimulateShareAsync(resourceId, shareRequest, cancellationToken).ConfigureAwait(false)).Value;
		var changes = simulation?.Changes;
		var addedUserIds = ExtractUserIds(changes?.Added);

		// Removal-only or permission-type-only changes keep the existing secrets.
		if (addedUserIds.Count > 0)
		{
			shareRequest.Secrets = await BuildSecretsForResultingAccessorsAsync(
				resourceId,
				payload,
				addedUserIds,
				changes?.Removed,
				cancellationToken).ConfigureAwait(false);
		}

		var response = await Resources.ShareAsync(resourceId, shareRequest, cancellationToken).ConfigureAwait(false);

		// The share endpoint does not always echo the resource body; re-fetch to return a consistent result.
		Resource? shared = response.Value;
		return shared ?? (await Resources.GetAsync(resourceId, cancellationToken).ConfigureAwait(false)).Value;
	}
}
