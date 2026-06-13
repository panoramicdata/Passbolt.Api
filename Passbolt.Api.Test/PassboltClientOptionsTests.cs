namespace Passbolt.Api.Test;

/// <summary>
/// Tests for Passbolt client defaults.
/// </summary>
public sealed class PassboltClientOptionsTests
{
	/// <summary>
	/// Verifies default option values are set.
	/// </summary>
	[Fact]
	public void Defaults_AreExpected()
	{
		var options = new PassboltClientOptions
		{
			Uri = new Uri("https://passbolt.panoramicdata.com"),
			Username = "testuser",
			Password = "testpassword",
			PrivateKeyBlock = "testprivatekeyblock"
		};

		options.Uri.Should().Be(new Uri("https://passbolt.panoramicdata.com"));
		options.Timeout.Should().Be(TimeSpan.FromSeconds(30));
	}
}
