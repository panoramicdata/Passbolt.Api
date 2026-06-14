namespace Passbolt.Api.Test;

/// <summary>
/// Unit tests for data model serialization and deserialization using System.Text.Json.
/// </summary>
public sealed class DataModelSerializationTests
{
	/// <summary>
	/// Verifies that Comment serializes and deserializes correctly.
	/// </summary>
	[Fact]
	public void Comment_SerializesAndDeserializes()
	{
		// Arrange
		var comment = new Comment
		{
			Id = "comment-1",
			ResourceId = "resource-1",
			UserId = "user-1",
			Content = "Test comment",
			CreatedTimestamp = DateTimeOffset.UtcNow,
			ModifiedTimestamp = DateTimeOffset.UtcNow
		};

		// Act
		var json = JsonSerializer.Serialize(comment);
		var deserialized = JsonSerializer.Deserialize<Comment>(json);

		// Assert
		deserialized.Should().NotBeNull();
		deserialized!.Content.Should().Be("Test comment");
		deserialized.ResourceId.Should().Be("resource-1");
	}

	/// <summary>
	/// Verifies that Avatar serializes and deserializes correctly.
	/// </summary>
	[Fact]
	public void Avatar_SerializesAndDeserializes()
	{
		// Arrange
		var avatar = new Avatar
		{
			Id = "avatar-1",
			UserId = "user-1",
			Image = "base64data",
			Url = "https://example.com/avatar.png",
			CreatedTimestamp = DateTimeOffset.UtcNow,
			ModifiedTimestamp = DateTimeOffset.UtcNow
		};

		// Act
		var json = JsonSerializer.Serialize(avatar);
		var deserialized = JsonSerializer.Deserialize<Avatar>(json);

		// Assert
		deserialized.Should().NotBeNull();
		deserialized!.Url.Should().Be("https://example.com/avatar.png");
	}

	/// <summary>
	/// Verifies that Role serializes and deserializes correctly.
	/// </summary>
	[Fact]
	public void Role_SerializesAndDeserializes()
	{
		// Arrange
		var role = new Role
		{
			Id = "role-1",
			Name = "Admin",
			Description = "Administrator",
			CreatedTimestamp = DateTimeOffset.UtcNow,
			ModifiedTimestamp = DateTimeOffset.UtcNow
		};

		// Act
		var json = JsonSerializer.Serialize(role);
		var deserialized = JsonSerializer.Deserialize<Role>(json);

		// Assert
		deserialized.Should().NotBeNull();
		deserialized!.Name.Should().Be("Admin");
	}

	/// <summary>
	/// Verifies that Permission serializes and deserializes correctly.
	/// </summary>
	[Fact]
	public void Permission_SerializesAndDeserializes()
	{
		// Arrange
		var permission = new Permission
		{
			Id = "perm-1",
			Aco = "Resource",
			AcoForeignKey = "res-1",
			Aro = "User",
			AroForeignKey = "user-1",
			Type = 7,
			CreatedTimestamp = DateTimeOffset.UtcNow,
			ModifiedTimestamp = DateTimeOffset.UtcNow
		};

		// Act
		var json = JsonSerializer.Serialize(permission);
		var deserialized = JsonSerializer.Deserialize<Permission>(json);

		// Assert
		deserialized.Should().NotBeNull();
		deserialized!.Type.Should().Be(7);
	}

	/// <summary>
	/// Verifies that CreateCommentRequest serializes correctly.
	/// </summary>
	[Fact]
	public void CreateCommentRequest_SerializesCorrectly()
	{
		// Arrange
		var request = new CreateCommentRequest
		{
			ResourceId = "resource-1",
			Content = "New comment"
		};

		// Act
		var json = JsonSerializer.Serialize(request);

		// Assert
		json.Should().Contain("resource-1");
		json.Should().Contain("New comment");
	}

	/// <summary>
	/// Verifies that IdentifiedItem handles null timestamps correctly.
	/// </summary>
	[Fact]
	public void IdentifiedItem_HandlesNullTimestamps()
	{
		// Arrange
		var role = new Role
		{
			Id = "role-1",
			Name = "User",
			CreatedTimestamp = null,
			ModifiedTimestamp = null
		};

		// Act
		var json = JsonSerializer.Serialize(role);
		var deserialized = JsonSerializer.Deserialize<Role>(json);

		// Assert
		deserialized.Should().NotBeNull();
		deserialized!.CreatedTimestamp.Should().BeNull();
		deserialized.ModifiedTimestamp.Should().BeNull();
	}

	/// <summary>
	/// Verifies that Header supports and preserves additional JSON properties.
	/// </summary>
	[Fact]
	public void Header_SupportsAdditionalData()
	{
		// Arrange
		var json = """
		{
			"status": "success",
			"custom_field": "custom_value",
			"another_field": 123
		}
		""";

		// Act
		var header = JsonSerializer.Deserialize<Header>(json);

		// Assert
		header.Should().NotBeNull();
		header!.Status.Should().Be("success");
		header.AdditionalProperties.Should().NotBeNull();
		header.AdditionalProperties.Should().ContainKey("custom_field");
	}
}
