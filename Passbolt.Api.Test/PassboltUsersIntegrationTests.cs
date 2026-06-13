using System.Text.Json;

namespace Passbolt.Api.Test;

/// <summary>
/// Integration tests for user use cases.
/// </summary>
public sealed class PassboltUsersIntegrationTests(ITestOutputHelper testOutputHelper, Fixture fixture) : TestBase(testOutputHelper, fixture)
{
	/// <summary>
	/// Ensures list-users is callable against a configured environment.
	/// </summary>
	[Fact]
	public async Task ListUsers_ReturnsEnvelope()
	{
		Settings.IsAuthenticatedConfigured.Should().BeTrue("Set Passbolt:Username, Passbolt:Password, and Passbolt:PrivateKeyBlock in user secrets to run authenticated integration tests.");

		var response = await Client
			.Users
			.GetAllAsync(CancellationToken);

		response.Should().NotBeNull();
		response.Header.Should().NotBeNull();
		response.Value.Should().NotBeNull();
		response.Value.Should().NotBeEmpty();
	}

	/// <summary>
	/// Ensures get-user works for the currently configured authenticated user.
	/// </summary>
	[Fact]
	public async Task GetUser_ReturnsEnvelope_ForConfiguredUser()
	{
		Settings.IsAuthenticatedConfigured.Should().BeTrue("Set Passbolt:Username, Passbolt:Password, and Passbolt:PrivateKeyBlock in user secrets to run authenticated integration tests.");
		var currentUser = await GetCurrentUserAsync();

		var response = await Client
			.Users
			.GetAsync(currentUser.Id!, CancellationToken);

		response.Should().NotBeNull();
		response.Header.Should().NotBeNull();
		response.Value.Should().NotBeNull();
		response.Value.Id.Should().Be(currentUser.Id);
	}

	/// <summary>
	/// Exercises update-user callability for the currently configured user.
	/// </summary>
	[Fact]
	public async Task Mutating_UserLifecycle_IsCallable_WhenEnabled()
	{
		Settings.IsAuthenticatedConfigured.Should().BeTrue("Set Passbolt:Username, Passbolt:Password, and Passbolt:PrivateKeyBlock in user secrets to run authenticated integration tests.");
		if (!Settings.RunMutatingIntegrationTests)
		{
			TestOutputHelper.WriteLine("Skipping mutating user lifecycle test because Passbolt:RunMutatingIntegrationTests is false.");
			return;
		}

		var currentUser = await GetCurrentUserAsync();

		var updateRequest = new Requests.UpdateUserRequest
		{
			FirstName = TryGetProfileProperty(currentUser, "first_name"),
			LastName = TryGetProfileProperty(currentUser, "last_name"),
			RoleId = TryGetRoleId(currentUser)
		};

		var updated = await Client
			.Users
			.UpdateAsync(currentUser.Id!, updateRequest, CancellationToken);

		updated.Should().NotBeNull();
		updated.Header.Should().NotBeNull();
		updated.Value.Should().NotBeNull();
		updated.Value.Id.Should().Be(currentUser.Id);
	}

	private static string? TryGetRoleId(Data.User user)
	{
		if (user.Role is not { ValueKind: JsonValueKind.Object } role)
		{
			return null;
		}

		return role.TryGetProperty("id", out var roleIdElement)
			? roleIdElement.GetString()
			: null;
	}

	private static string? TryGetProfileProperty(Data.User user, string propertyName)
	{
		if (user.Profile is not { ValueKind: JsonValueKind.Object } profile)
		{
			return null;
		}

		return profile.TryGetProperty(propertyName, out var propertyValue)
			? propertyValue.GetString()
			: null;
	}
}
