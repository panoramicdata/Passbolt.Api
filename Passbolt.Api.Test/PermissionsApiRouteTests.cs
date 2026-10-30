using Passbolt.Api.Apis;

namespace Passbolt.Api.Test;

/// <summary>
/// Pins the permissions routes to the endpoints Passbolt actually serves.
/// </summary>
public sealed class PermissionsApiRouteTests
{
	/// <summary>Observed live on 2026-10-30: this route returned the ACL; /permissions?filter[resource_id]= returned 404.</summary>
	[Fact]
	public void GetByResourceAsync_UsesResourcePermissionsEndpoint()
		=> RouteOf(nameof(IPassboltPermissionsApi.GetByResourceAsync))
			.Should().Be("/permissions/resource/{resourceId}.json");

	/// <summary>Both routes returned 404 against a live Passbolt server on 2026-10-30.</summary>
	/// <param name="methodName">The interface method under test.</param>
	[Theory]
	[InlineData(nameof(IPassboltPermissionsApi.GetAllAsync))]
	[InlineData(nameof(IPassboltPermissionsApi.GetByUserAsync))]
	public void EndpointsPassboltDoesNotServe_AreObsolete(string methodName)
		=> typeof(IPassboltPermissionsApi).GetMethod(methodName)!
			.GetCustomAttributes(typeof(ObsoleteAttribute), false)
			.Should().ContainSingle();

	private static string RouteOf(string methodName)
		=> typeof(IPassboltPermissionsApi).GetMethod(methodName)!
			.GetCustomAttributes(typeof(Refit.GetAttribute), false)
			.Cast<Refit.GetAttribute>()
			.Single()
			.Path;
}
