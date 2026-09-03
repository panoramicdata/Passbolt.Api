namespace Passbolt.Api.Test;

/// <summary>
/// Integration tests for group use cases.
/// </summary>
public sealed class PassboltGroupsIntegrationTests(ITestOutputHelper testOutputHelper, Fixture fixture) : TestBase(testOutputHelper, fixture)
{
	/// <summary>
	/// Ensures list-groups is callable against a configured environment.
	/// </summary>
	[Fact]
	public async Task ListGroups_ReturnsEnvelope()
	{
		var response = await Client.Groups.ListAsync(CancellationToken);

		response.Should().NotBeNull();
		response.Header.Should().NotBeNull();
	}

	/// <summary>
	/// Ensures get-group works for a newly created group.
	/// </summary>
	[Fact]
	public async Task GetGroup_ReturnsEnvelope_ForCreatedGroup()
	{
		var (createdGroupId, _) = await CreateIntegrationGroupAsync();

		try
		{
			var response = await Client.Groups.GetAsync(createdGroupId, CancellationToken);

			response.Should().NotBeNull();
			response.Header.Should().NotBeNull();
			response.Value.Should().NotBeNull();
			response.Value.Id.Should().Be(createdGroupId);
		}
		finally
		{
			await Client.Groups.DeleteAsync(createdGroupId, CancellationToken);
		}
	}

	/// <summary>
	/// Exercises create/update/delete group lifecycle when mutating tests are enabled.
	/// </summary>
	[Fact]
	public async Task Mutating_GroupLifecycle_IsCallable_WhenEnabled()
	{
		if (!Settings.RunMutatingIntegrationTests)
		{
			TestOutputHelper.WriteLine("Skipping mutating group lifecycle test because Passbolt:RunMutatingIntegrationTests is false.");
			return;
		}

		var (createdGroupId, ownerUserId) = await CreateIntegrationGroupAsync();

		try
		{
			await Client.Groups.UpdateAsync(createdGroupId, new Requests.UpdateGroupRequest
			{
				Name = $"Passbolt.Api Integration Group Updated {Guid.NewGuid():N}",
				GroupUsers = [AdminMembership(ownerUserId)]
			}, CancellationToken);
		}
		finally
		{
			await Client.Groups.DeleteAsync(createdGroupId, CancellationToken);
		}
	}

	/// <summary>
	/// Creates a throwaway group administered by the configured user, returning its id alongside
	/// that user's id (which callers need to re-state the membership on update).
	/// </summary>
	private async Task<(string GroupId, string OwnerUserId)> CreateIntegrationGroupAsync()
	{
		var currentUser = await GetCurrentUserAsync();
		var created = await Client.Groups.CreateAsync(new Requests.CreateGroupRequest
		{
			Name = $"Passbolt.Api Integration Group {Guid.NewGuid():N}",
			GroupUsers = [AdminMembership(currentUser.Id!)]
		}, CancellationToken);

		var createdGroupId = created.Value.Id;
		createdGroupId.Should().NotBeNullOrWhiteSpace();
		return (createdGroupId!, currentUser.Id!);
	}

	private static Requests.GroupUserMembershipRequest AdminMembership(string userId)
		=> new() { UserId = userId, IsAdmin = true };
}
