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
		Settings.IsAuthenticatedConfigured.Should().BeTrue("Set Passbolt:Username, Passbolt:Password, and Passbolt:PrivateKeyBlock in user secrets to run authenticated integration tests.");
		if (!Settings.RunMutatingIntegrationTests)
		{
			TestOutputHelper.WriteLine("Skipping mutating resource lifecycle test because Passbolt:RunMutatingIntegrationTests is false.");
			return;
		}

		var parentFolderId = await EnsureTestFolderExistsAsync();
		var sharedGroupId = await EnsureTestGroupExistsAsync();

		var originalSecret = $"Secret-{Guid.NewGuid():N}";

		// Create with client-side PGP encryption (password-and-description type).
		var created = await Client.CreateResourceAsync(
			name: $"Passbolt.Api Integration Resource {Guid.NewGuid():N}",
			username: Settings.ResourceLookupUsername ?? "integration.user",
			uri: $"https://integration-{Guid.NewGuid():N}.example",
			password: originalSecret,
			description: "Passbolt.Api integration test resource",
			folderParentId: parentFolderId,
			encryptDescription: true,
			CancellationToken);

		var createdResourceId = created.Id;
		createdResourceId.Should().NotBeNullOrWhiteSpace();

		try
		{
			// Read the secret back and confirm it decrypts to what we encrypted.
			var afterCreate = (await Client.Secrets.GetForResourceAsync(createdResourceId!, CancellationToken)).Value;
			var decryptedAfterCreate = Cryptography.PassboltPgp.Decrypt(afterCreate.Data!, Settings.PrivateKeyBlock!, Settings.Password!);
			decryptedAfterCreate.Should().Contain(originalSecret);

			// Rotate the secret and confirm the new value round-trips.
			var rotatedSecret = $"Secret-{Guid.NewGuid():N}";
			await Client.RotateResourceSecretAsync(createdResourceId!, rotatedSecret, description: null, CancellationToken);
			var afterRotate = (await Client.Secrets.GetForResourceAsync(createdResourceId!, CancellationToken)).Value;
			var decryptedAfterRotate = Cryptography.PassboltPgp.Decrypt(afterRotate.Data!, Settings.PrivateKeyBlock!, Settings.Password!);
			decryptedAfterRotate.Should().Contain(rotatedSecret);
			decryptedAfterRotate.Should().NotContain(originalSecret);

			// Share with a group; recipients' secrets are re-encrypted automatically.
			await Client.ShareResourceAsync(createdResourceId!,
			[
				new Requests.SharePermissionRequest
				{
					IsNew = true,
					Aro = "Group",
					AroForeignKey = sharedGroupId,
					Aco = "Resource",
					AcoForeignKey = createdResourceId,
					Type = 1
				}
			], CancellationToken);
		}
		finally
		{
			await Client.Resources.DeleteAsync(createdResourceId!, CancellationToken);
		}
	}
}
