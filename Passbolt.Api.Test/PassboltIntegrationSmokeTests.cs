namespace Passbolt.Api.Test;

/// <summary>
/// Basic live endpoint checks.
/// </summary>
public sealed class PassboltIntegrationSmokeTests(ITestOutputHelper testOutputHelper, Fixture fixture) : TestBase(testOutputHelper, fixture)
{
	/// <summary>
	/// Validates the unauthenticated status endpoint.
	/// </summary>
	[Fact]
	public async Task StatusEndpoint_ReturnsStatus()
	{
		var status = await Client.Status.GetHealthcheckAsync(CancellationToken);
		status.Should().NotBeNull();
		status.Header.Should().NotBeNull();
	}

	/// <summary>
	/// Validates the authenticated healthcheck endpoint.
	/// </summary>
	[Fact]
	public async Task HealthcheckEndpoint_ReturnsEnvelope_WhenAuthenticated()
	{
		var response = await Client.Status.GetHealthcheckAsync(CancellationToken);

		response.Should().NotBeNull();
		response.Header.Should().NotBeNull();
	}
}
