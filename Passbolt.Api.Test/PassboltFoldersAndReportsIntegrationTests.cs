namespace Passbolt.Api.Test;

/// <summary>
/// Integration tests for folder and reporting use cases.
/// </summary>
public sealed class PassboltFoldersAndReportsIntegrationTests(ITestOutputHelper testOutputHelper, Fixture fixture) : TestBase(testOutputHelper, fixture)
{
	/// <summary>
	/// Ensures list-folders is callable against a configured environment.
	/// </summary>
	[Fact]
	public async Task ListFolders_ReturnsEnvelope()
	{
		var response = await Client.Folders.ListFoldersAsync(CancellationToken);

		response.Should().NotBeNull();
		response.Header.Should().NotBeNull();
	}

	/// <summary>
	/// Ensures get-folder works for the shared integration test folder.
	/// </summary>
	[Fact]
	public async Task GetFolder_ReturnsEnvelope_ForSharedTestFolder()
	{
		var folderId = await EnsureTestFolderExistsAsync();

		var response = await Client
			.Folders
			.GetFolderAsync(folderId, CancellationToken);

		response.Should().NotBeNull();
		response.Header.Should().NotBeNull();
	}

	/// <summary>
	/// Ensures user-group cross-reference report generation succeeds.
	/// </summary>
	[Fact]
	public async Task UserReport_ReturnsCollection()
	{
		var users = await Client
			.Users
			.GetAllAsync(CancellationToken);

		var groups = await Client
			.Groups
			.ListAsync(CancellationToken);

		users.Should().NotBeNull();
		users.Value.Should().NotBeNull();
		groups.Should().NotBeNull();
		groups.Value.Should().NotBeNull();
	}

	/// <summary>
	/// Exercises create/delete folder lifecycle when mutating tests are enabled.
	/// </summary>
	[Fact]
	public async Task Mutating_FolderLifecycle_IsCallable_WhenEnabled()
	{
		Settings.IsAuthenticatedConfigured.Should().BeTrue("Set Passbolt:Username, Passbolt:Password, and Passbolt:PrivateKeyBlock in user secrets to run authenticated integration tests.");
		if (!Settings.RunMutatingIntegrationTests)
		{
			TestOutputHelper.WriteLine("Skipping mutating folder lifecycle test because Passbolt:RunMutatingIntegrationTests is false.");
			return;
		}

		var created = await Client
			.Folders
			.CreateFolderAsync(
				new Requests.CreateFolderRequest
				{
					Name = $"Passbolt.Api Integration Folder {Guid.NewGuid():N}"
				},
				CancellationToken);

		var folderId = created.Value.Id;
		folderId.Should().NotBeNullOrWhiteSpace();

		await Client.Folders.DeleteFolderAsync(folderId!, CancellationToken);
	}
}
