namespace Passbolt.Api.Test;

/// <summary>
/// Unit tests for PassboltClient initialization and disposal.
/// </summary>
public sealed class PassboltClientUnitTests
{
	private readonly PassboltClientOptions _validOptions = new()
	{
		Uri = new Uri("https://example.com"),
		Username = "user@example.com",
		Password = "password",
		PrivateKeyBlock = "-----BEGIN PGP PRIVATE KEY BLOCK-----\nkey\n-----END PGP PRIVATE KEY BLOCK-----"
	};

	[Fact]
	public void Constructor_CreatesInternalHttpClient_WhenNotProvided()
	{
		// Act
		using var client = new PassboltClient(_validOptions);

		// Assert
		client.Status.Should().NotBeNull();
		client.Users.Should().NotBeNull();
		client.Groups.Should().NotBeNull();
		client.Resources.Should().NotBeNull();
		client.Folders.Should().NotBeNull();
		client.Comments.Should().NotBeNull();
		client.Permissions.Should().NotBeNull();
		client.Me.Should().NotBeNull();
		client.Avatars.Should().NotBeNull();
		client.Roles.Should().NotBeNull();
	}

	[Fact]
	public void Constructor_UsesProvidedHttpClient()
	{
		// Arrange
		using var httpClient = new HttpClient();
		_validOptions.HttpClient = httpClient;

		// Act
		using var client = new PassboltClient(_validOptions);

		// Assert
		client.Status.Should().NotBeNull();
	}

	[Fact]
	public void Constructor_ThrowsArgumentNullException_WhenOptionsNull()
	{
		// Act & Assert
		var action = () => new PassboltClient(null!);
		action.Should().Throw<ArgumentNullException>();
	}

	[Fact]
	public void Dispose_DisposesInternalHttpClient()
	{
		// Arrange
		var client = new PassboltClient(_validOptions);

		// Act
		client.Dispose();

		// Assert - should not throw
	}

	[Fact]
	public void Dispose_DoesNotDisposeProvidedHttpClient()
	{
		// Arrange
		using var httpClient = new HttpClient();
		_validOptions.HttpClient = httpClient;
		var client = new PassboltClient(_validOptions);

		// Act
		client.Dispose();

		// Assert - httpClient should still be usable
		httpClient.Should().NotBeNull();
	}
}
