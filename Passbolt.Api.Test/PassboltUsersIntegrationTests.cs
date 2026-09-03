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
		RequireAuthenticatedConfiguration();

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
		RequireAuthenticatedConfiguration();
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
		RequireAuthenticatedConfiguration();
		if (!Settings.RunMutatingIntegrationTests)
		{
			TestOutputHelper.WriteLine("Skipping mutating user lifecycle test because Passbolt:RunMutatingIntegrationTests is false.");
			return;
		}

		var currentUser = await GetCurrentUserAsync();

		var updateRequest = new Requests.UpdateUserRequest
		{
			FirstName = TryGetStringProperty(currentUser.Profile, "first_name"),
			LastName = TryGetStringProperty(currentUser.Profile, "last_name"),
			RoleId = TryGetStringProperty(currentUser.Role, "id")
		};

		var updated = await Client
			.Users
			.UpdateAsync(currentUser.Id!, updateRequest, CancellationToken);

		updated.Should().NotBeNull();
		updated.Header.Should().NotBeNull();
		updated.Value.Should().NotBeNull();
		updated.Value.Id.Should().Be(currentUser.Id);
	}

	/// <summary>
	/// Reads a string property from one of the loosely-typed JSON blobs on <see cref="Data.User"/>
	/// (profile, role), returning null when the blob or the property is absent.
	/// </summary>
	private static string? TryGetStringProperty(JsonElement? container, string propertyName)
		=> container is { ValueKind: JsonValueKind.Object } element
			&& element.TryGetProperty(propertyName, out var value)
				? value.GetString()
				: null;
}
