namespace Passbolt.Api.Test;

/// <summary>
/// Base class for integration tests that provides a configured Passbolt client and test output helper.
/// </summary>
public abstract class TestBase : TestBed<Fixture>
{
	private const string SharedTestFolderName = "Passbolt.Api Nuget Testing";
	private const string SharedTestGroupName = "Passbolt.Api Nuget Testing Group";

	/// <summary>
	/// The integration settings loaded from user secrets and configuration.
	/// </summary>
	protected PassboltIntegrationSettings Settings { get; }

	/// <summary>
	/// The Passbolt client instance to use for integration tests.
	/// </summary>
	protected readonly ITestOutputHelper TestOutputHelper;

	/// <summary>
	/// The Passbolt client instance to use for integration tests.
	/// </summary>
	protected PassboltClient Client { get; }

	/// <summary>
	/// The cancellation token to use for integration tests.
	/// </summary>
	protected CancellationToken CancellationToken  { get; } = TestContext.Current.CancellationToken;

	/// <summary>
	/// Initializes a new instance of the <see cref="TestBase"/> class with the specified test output helper.
	/// </summary>
	/// <param name="testOutputHelper"></param>
	/// <param name="fixture"></param>
	protected TestBase(ITestOutputHelper testOutputHelper, Fixture fixture) : base(testOutputHelper, fixture)
	{
		var options = fixture.GetService<IOptions<PassboltIntegrationSettings>>(testOutputHelper)
			?? throw new InvalidOperationException("Passbolt integration settings are not configured.");

		Settings = options.Value;
		TestOutputHelper = testOutputHelper;
		var logger = new TestOutputHelperLogger<PassboltClient>(testOutputHelper, LogLevel.Debug);
		Client = Settings.CreateClient(logger);
	}

	/// <summary>
	/// Ensures the shared integration test folder exists and returns its identifier.
	/// </summary>
	/// <returns>The folder identifier for the shared integration test folder.</returns>
	protected Task<string> EnsureTestFolderExistsAsync()
	{
		var serverKey = Settings.ServerUrl.Trim().ToLowerInvariant();
		var cacheKey = $"{serverKey}|{SharedTestFolderName}";
		return TestFolderCacheService.GetOrAddAsync(cacheKey, EnsureTestFolderExistsCoreAsync);
	}

	/// <summary>
	/// Ensures the shared integration test group exists and returns its identifier.
	/// </summary>
	/// <returns>The group identifier for the shared integration test group.</returns>
	protected Task<string> EnsureTestGroupExistsAsync()
	{
		var serverKey = Settings.ServerUrl.Trim().ToLowerInvariant();
		var cacheKey = $"{serverKey}|{SharedTestGroupName}";
		return TestFolderCacheService.GetOrAddAsync(cacheKey, EnsureTestGroupExistsCoreAsync);
	}

	/// <summary>
	/// Resolves the current configured user from the Passbolt user list.
	/// </summary>
	/// <returns>The authenticated user model.</returns>
	protected async Task<Data.User> GetCurrentUserAsync()
	{
		Settings.Username.Should().NotBeNullOrWhiteSpace("Passbolt:Username must be configured for authenticated integration tests.");

		var users = await Client.Users.GetAllAsync(CancellationToken);
		users.Should().NotBeNull();
		users.Value.Should().NotBeNull();

		var configuredUsername = Settings.Username!;
		var currentUser = users.Value
			.FirstOrDefault(user => string.Equals(user.Username, configuredUsername, StringComparison.OrdinalIgnoreCase));

		if (currentUser is null && !configuredUsername.Contains('@'))
		{
			currentUser = users.Value.FirstOrDefault(user =>
			{
				if (string.IsNullOrWhiteSpace(user.Username))
				{
					return false;
				}

				var atIndex = user.Username.IndexOf('@');
				var localPart = atIndex >= 0
					? user.Username[..atIndex]
					: user.Username;

				return localPart.StartsWith(configuredUsername, StringComparison.OrdinalIgnoreCase);
			});
		}

		currentUser.Should().NotBeNull($"Expected to find configured user '{configuredUsername}' in the users list response.");
		currentUser!.Id.Should().NotBeNullOrWhiteSpace("Current user must include an id.");

		return currentUser;
	}

	private async Task<string> EnsureTestFolderExistsCoreAsync()
	{
		var folders = await Client.Folders.ListFoldersAsync(CancellationToken);
		folders.Should().NotBeNull();
		folders.Value.Should().NotBeNull();

		foreach (var folder in folders.Value)
		{
			if (!string.Equals(folder.Name, SharedTestFolderName, StringComparison.OrdinalIgnoreCase))
			{
				continue;
			}

			if (!string.IsNullOrWhiteSpace(folder.Id))
			{
				return folder.Id;
			}
		}

		var created = await Client.Folders.CreateFolderAsync(new Requests.CreateFolderRequest
		{
			Name = SharedTestFolderName
		}, CancellationToken);

		created.Should().NotBeNull();
		created.Value.Should().NotBeNull();
		created.Value.Id.Should().NotBeNullOrWhiteSpace("created folder responses must include an id");
		return created.Value.Id!;
	}

	private async Task<string> EnsureTestGroupExistsCoreAsync()
	{
		var groups = await Client.Groups.ListAsync(CancellationToken);
		groups.Should().NotBeNull();
		groups.Value.Should().NotBeNull();

		foreach (var group in groups.Value)
		{
			if (!string.Equals(group.Name, SharedTestGroupName, StringComparison.OrdinalIgnoreCase))
			{
				continue;
			}

			if (!string.IsNullOrWhiteSpace(group.Id))
			{
				return group.Id;
			}
		}

		var currentUser = await GetCurrentUserAsync();
		var created = await Client.Groups.CreateAsync(new Requests.CreateGroupRequest
		{
			Name = SharedTestGroupName,
			GroupUsers =
			[
				new Requests.GroupUserMembershipRequest
				{
					UserId = currentUser.Id!,
					IsAdmin = true
				}
			]
		}, CancellationToken);

		created.Should().NotBeNull();
		created.Value.Should().NotBeNull();
		created.Value.Id.Should().NotBeNullOrWhiteSpace("created group responses must include an id");
		return created.Value.Id!;
	}

	/// <summary>
	/// Disposes the test client and then disposes fixture-backed test resources.
	/// </summary>
	/// <returns>A task representing asynchronous disposal completion.</returns>
	public new virtual ValueTask DisposeAsync()
	{
		Client.Dispose();
		return base.DisposeAsync();
	}
}
