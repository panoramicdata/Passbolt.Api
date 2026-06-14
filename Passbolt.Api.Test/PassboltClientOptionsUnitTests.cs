namespace Passbolt.Api.Test;

/// <summary>
/// Unit tests for PassboltClientOptions validation and initialization.
/// </summary>
public sealed class PassboltClientOptionsUnitTests
{
	/// <summary>
	/// Verifies that PassboltClientOptions initializes with all required properties and default values.
	/// </summary>
	[Fact]
	public void Constructor_WithAllRequiredProperties()
	{
		// Arrange & Act
		var options = new PassboltClientOptions
		{
			Uri = new Uri("https://example.com"),
			Username = "user@example.com",
			Password = "password",
			PrivateKeyBlock = "[PGP PRIVATE KEY BLOCK - Export from Passbolt: Menu > My Account > Security > Download private key]"
		};

		// Assert
		options.Uri.Should().Be(new Uri("https://example.com"));
		options.Username.Should().Be("user@example.com");
		options.Password.Should().Be("password");
		options.PrivateKeyBlock.Should().StartWith("[PGP PRIVATE KEY BLOCK");
		options.Timeout.Should().Be(TimeSpan.FromSeconds(30));
		options.Logger.Should().NotBeNull();
		options.HttpClient.Should().BeNull();
	}

	/// <summary>
	/// Verifies that custom timeout can be set in PassboltClientOptions.
	/// </summary>
	[Fact]
	public void Constructor_WithCustomTimeout()
	{
		// Arrange
		var customTimeout = TimeSpan.FromSeconds(60);

		// Act
		var options = new PassboltClientOptions
		{
			Uri = new Uri("https://example.com"),
			Username = "user@example.com",
			Password = "password",
			PrivateKeyBlock = "key",
			Timeout = customTimeout
		};

		// Assert
		options.Timeout.Should().Be(customTimeout);
	}

	/// <summary>
	/// Verifies that a custom logger can be provided to PassboltClientOptions.
	/// </summary>
	[Fact]
	public void Constructor_WithCustomLogger()
	{
		// Arrange
		var mockLogger = new TestLogger();

		// Act
		var options = new PassboltClientOptions
		{
			Uri = new Uri("https://example.com"),
			Username = "user@example.com",
			Password = "password",
			PrivateKeyBlock = "key",
			Logger = mockLogger
		};

		// Assert
		options.Logger.Should().Be(mockLogger);
	}

	/// <summary>
	/// Verifies that a custom HttpClient can be provided to PassboltClientOptions.
	/// </summary>
	[Fact]
	public void Constructor_WithCustomHttpClient()
	{
		// Arrange
		using var customClient = new HttpClient();

		// Act
		var options = new PassboltClientOptions
		{
			Uri = new Uri("https://example.com"),
			Username = "user@example.com",
			Password = "password",
			PrivateKeyBlock = "key",
			HttpClient = customClient
		};

		// Assert
		options.HttpClient.Should().Be(customClient);
	}

	private sealed class TestLogger : ILogger
	{
		public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
		{
			// Mock logger for testing - no-op implementation
		}

		public bool IsEnabled(LogLevel _) => false;

		public IDisposable? BeginScope<TState>(TState _) where TState : notnull => null;
	}
}
