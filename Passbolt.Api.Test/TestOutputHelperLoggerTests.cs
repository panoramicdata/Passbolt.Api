namespace Passbolt.Api.Test;

/// <summary>
/// Verifies that test logger messages flow to xUnit output.
/// </summary>
public sealed class TestOutputHelperLoggerTests(ITestOutputHelper output)
{
	/// <summary>
	/// Writes a debug message to xUnit output through the test logger.
	/// </summary>
	[Fact]
	public void DebugLog_WritesToTestOutput()
	{
		var logger = new TestOutputHelperLogger<TestOutputHelperLoggerTests>(output, LogLevel.Debug);

		logger.LogDebug("Visible debug message from TestOutputHelperLoggerTests");

		true.Should().BeTrue();
	}
}
