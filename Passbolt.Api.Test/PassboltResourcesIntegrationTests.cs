namespace Passbolt.Api.Test;

/// <summary>
/// Integration tests for resource/password use cases.
/// </summary>
public sealed class PassboltResourcesIntegrationTests(ITestOutputHelper testOutputHelper, Fixture fixture) : TestBase(testOutputHelper, fixture)
{
	/// <summary>
	/// Ensures list-resources is callable against a configured environment.
	/// </summary>
	[Fact]
	public async Task ListResources_ReturnsEnvelope()
	{
		var resources = await Client.Resources.GetAllAsync(CancellationToken);

		resources.Should().NotBeNull();
		resources.Header.Should().NotBeNull();
		resources.Value.Should().NotBeNull();
	}

	/// <summary>
	/// Ensures list-resources and get-resource-by-id are callable for existing resources.
	/// </summary>
	[Fact]
	public async Task GetResource_ReturnsEnvelope_ForExistingResource()
	{
		var resources = await Client.Resources.GetAllAsync(CancellationToken);
		resources.Should().NotBeNull();
		resources.Value.Should().NotBeNull();
		resources.Value.Should().NotBeEmpty();

		var selected = resources.Value.First(resource => !string.IsNullOrWhiteSpace(resource.Id));

		var byId = await Client.Resources.GetAsync(selected.Id!, CancellationToken);
		byId.Should().NotBeNull();
		byId.Header.Should().NotBeNull();
		byId.Value.Should().NotBeNull();
		byId.Value.Id.Should().Be(selected.Id);
	}

	/// <summary>
	/// Exercises create/update/share/delete resource lifecycle when mutating tests are enabled.
	/// </summary>
	[Fact]
	public async Task Mutating_ResourceLifecycle_AndShare_AreCallable_WhenEnabled()
	{
		RequireAuthenticatedConfiguration();
		if (!Settings.RunMutatingIntegrationTests)
		{
			TestOutputHelper.WriteLine("Skipping mutating resource lifecycle test because Passbolt:RunMutatingIntegrationTests is false.");
			return;
		}

		var parentFolderId = await EnsureTestFolderExistsAsync();
		var sharedGroupId = await EnsureTestGroupExistsAsync();

		var originalSecret = $"Secret-{Guid.NewGuid():N}";
		var created = await CreateIntegrationResourceAsync(parentFolderId, originalSecret);

		var createdResourceId = created.Id;
		createdResourceId.Should().NotBeNullOrWhiteSpace();

		try
		{
			// Read the secret back and confirm it decrypts to what we encrypted.
			(await DecryptResourceSecretAsync(createdResourceId!)).Should().Contain(originalSecret);

			// Rotate the secret and confirm the new value round-trips.
			var rotatedSecret = $"Secret-{Guid.NewGuid():N}";
			await Client.RotateResourceSecretAsync(createdResourceId!, rotatedSecret, description: null, CancellationToken);
			var decryptedAfterRotate = await DecryptResourceSecretAsync(createdResourceId!);
			decryptedAfterRotate.Should().Contain(rotatedSecret);
			decryptedAfterRotate.Should().NotContain(originalSecret);

			// Share with a group; recipients' secrets are re-encrypted automatically.
			await ShareWithGroupAsync(createdResourceId!, sharedGroupId);
		}
		finally
		{
			await Client.Resources.DeleteAsync(createdResourceId!, CancellationToken);
		}
	}

	/// <summary>Creates a throwaway resource of the password-and-description type for the lifecycle test.</summary>
	private Task<Data.Resource> CreateIntegrationResourceAsync(string parentFolderId, string password)
		=> Client.CreateResourceAsync(
			name: $"Passbolt.Api Integration Resource {Guid.NewGuid():N}",
			username: Settings.ResourceLookupUsername ?? "integration.user",
			uri: $"https://integration-{Guid.NewGuid():N}.example",
			password: password,
			description: "Passbolt.Api integration test resource",
			folderParentId: parentFolderId,
			encryptDescription: true,
			CancellationToken);

	/// <summary>Reads a resource's secret back and decrypts it with the configured private key.</summary>
	private async Task<string> DecryptResourceSecretAsync(string resourceId)
	{
		var secret = (await Client.Secrets.GetForResourceAsync(resourceId, CancellationToken)).Value;
		return Cryptography.PassboltPgp.Decrypt(secret.Data!, Settings.PrivateKeyBlock!, Settings.Password!);
	}

	/// <summary>Grants a group read access to a resource, re-encrypting the secret for its members.</summary>
	private Task ShareWithGroupAsync(string resourceId, string groupId)
		=> Client.ShareResourceAsync(resourceId,
		[
			new Requests.SharePermissionRequest
			{
				IsNew = true,
				Aro = "Group",
				AroForeignKey = groupId,
				Aco = "Resource",
				AcoForeignKey = resourceId,
				Type = 1
			}
		], CancellationToken);
}
