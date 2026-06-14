namespace Passbolt.Api.Test;

/// <summary>
/// Unit tests for PassboltClient initialization and validation.
/// </summary>
public sealed class PassboltClientUnitTests
{
	/// <summary>
	/// Verifies that PassboltClient throws ArgumentNullException when options are null.
	/// </summary>
	[Fact]
	public void Constructor_ThrowsWhenOptionsNull()
	{
		// Act & Assert
		Assert.Throws<ArgumentNullException>(() => new PassboltClient(null!));
	}

	/// <summary>
	/// Verifies that PassboltClientOptions allows setting a custom HttpClient.
	/// </summary>
	[Fact]
	public void Options_AllowsCustomHttpClient()
	{
		// Arrange
		using var httpClient = new HttpClient();
		var options = new PassboltClientOptions
		{
			Uri = new Uri("https://example.com"),
			Username = "user@example.com",
			Password = "password",
			PrivateKeyBlock = "[PGP PRIVATE KEY BLOCK - Export from Passbolt: Menu > My Account > Security > Download private key]",
			HttpClient = httpClient
		};

		// Assert
		options.HttpClient.Should().Be(httpClient);
	}

	/// <summary>
	/// Verifies that PassboltClientOptions default timeout is 30 seconds.
	/// </summary>
	[Fact]
	public void Options_DefaultTimeoutIs30Seconds()
	{
		// Arrange
		var options = new PassboltClientOptions
		{
			Uri = new Uri("https://example.com"),
			Username = "user@example.com",
			Password = "password",
			PrivateKeyBlock = "[PGP PRIVATE KEY BLOCK - Export from Passbolt: Menu > My Account > Security > Download private key]"
		};

		// Assert
		options.Timeout.Should().Be(TimeSpan.FromSeconds(30));
	}

	/// <summary>
	/// Verifies that PassboltClientOptions has a default logger.
	/// </summary>
	[Fact]
	public void Options_HasDefaultLogger()
	{
		// Arrange
		var options = new PassboltClientOptions
		{
			Uri = new Uri("https://example.com"),
			Username = "user@example.com",
			Password = "password",
			PrivateKeyBlock = "[PGP PRIVATE KEY BLOCK - Export from Passbolt: Menu > My Account > Security > Download private key]"
		};

		// Assert
		options.Logger.Should().NotBeNull();
	}
}
