using Passbolt.Api.Apis;

namespace Passbolt.Api.Test;

/// <summary>
/// Pins the share-simulation and group-membership shapes to what Passbolt actually returns.
/// </summary>
public sealed class ShareSimulationDeserializationTests
{
	/// <summary>Shape returned by /share/simulate/resource/{id}.json on a live server, 2026-10-30.</summary>
	[Fact]
	public void SimulationChange_ReadsNestedUserId()
	{
		const string json = """{"changes":{"added":[{"user":{"id":"u-1"}}],"removed":[{"user":{"id":"u-2"}}]}}""";

		var result = JsonSerializer.Deserialize<ShareSimulationResult>(json)!;

		result.Changes!.Added!.Single().EffectiveUserId.Should().Be("u-1");
		result.Changes.Removed!.Single().EffectiveUserId.Should().Be("u-2");
	}

	/// <summary>The flat form is still read, in case a server sends it.</summary>
	[Fact]
	public void SimulationChange_FallsBackToFlatUserId()
		=> JsonSerializer.Deserialize<ShareSimulationSecret>("""{"user_id":"u-3"}""")!
			.EffectiveUserId.Should().Be("u-3");

	/// <summary>Without contain[user] Passbolt returns a group with no members.</summary>
	[Fact]
	public void GetGroup_AsksForMembers()
		=> typeof(IPassboltGroupsApi).GetMethod(nameof(IPassboltGroupsApi.GetAsync))!
			.GetCustomAttributes(typeof(Refit.GetAttribute), false)
			.Cast<Refit.GetAttribute>()
			.Single()
			.Path.Should().Contain("contain[user]=1");
}
