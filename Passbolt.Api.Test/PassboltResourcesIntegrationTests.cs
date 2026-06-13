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

		var created = await Client.Resources.CreateAsync(new Requests.CreateResourceRequest
		{
			Name = $"Passbolt.Api Integration Resource {Guid.NewGuid():N}",
			Username = Settings.ResourceLookupUsername ?? "integration.user",
			Uri = $"https://integration-{Guid.NewGuid():N}.example",
			Secret = $"Secret-{Guid.NewGuid():N}",
			Description = "Passbolt.Api integration test resource",
			ParentFolderId = parentFolderId
		}, CancellationToken);

		var createdResourceId = created.Value.Id;
		createdResourceId.Should().NotBeNullOrWhiteSpace();

		try
		{
			await Client.Resources.UpdateAsync(createdResourceId!, new Requests.UpdateResourceRequest
			{
				Name = $"Passbolt.Api Integration Resource Updated {Guid.NewGuid():N}",
				Description = "Updated by integration tests"
			}, CancellationToken);

			await Client.Resources.ShareAsync(createdResourceId!, new Requests.ShareResourceRequest
			{
				Permissions =
				[
					new Requests.SharePermissionRequest
					{
						GroupId = sharedGroupId,
						Type = 1
					}
				]
			}, CancellationToken);
		}
		finally
		{
			await Client.Resources.DeleteAsync(createdResourceId!, CancellationToken);
		}
	}
}
