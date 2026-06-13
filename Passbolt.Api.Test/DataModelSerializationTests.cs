namespace Passbolt.Api.Test;

/// <summary>
/// Unit tests for data model serialization and deserialization.
/// </summary>
public sealed class DataModelSerializationTests
{
	[Fact]
	public void Comment_SerializesAndDeserializes()
	{
		// Arrange
		var comment = new Comment
		{
			Id = "comment-id",
			ResourceId = "resource-id",
			UserId = "user-id",
			Content = "Test comment",
			CreatedTimestamp = DateTimeOffset.UtcNow,
			ModifiedTimestamp = DateTimeOffset.UtcNow
		};

		// Act
		var json = JsonSerializer.Serialize(comment);
		var deserialized = JsonSerializer.Deserialize<Comment>(json);

		// Assert
		deserialized.Should().NotBeNull();
		deserialized!.Id.Should().Be(comment.Id);
		deserialized.Content.Should().Be(comment.Content);
	}

	[Fact]
	public void Avatar_SerializesAndDeserializes()
	{
		// Arrange
		var avatar = new Avatar
		{
			Id = "avatar-id",
			UserId = "user-id",
			Image = "base64-encoded-image",
			Url = "https://example.com/avatar.png",
			CreatedTimestamp = DateTimeOffset.UtcNow,
			ModifiedTimestamp = DateTimeOffset.UtcNow
		};

		// Act
		var json = JsonSerializer.Serialize(avatar);
		var deserialized = JsonSerializer.Deserialize<Avatar>(json);

		// Assert
		deserialized.Should().NotBeNull();
		deserialized!.Id.Should().Be(avatar.Id);
		deserialized.Url.Should().Be(avatar.Url);
	}

	[Fact]
	public void Role_SerializesAndDeserializes()
	{
		// Arrange
		var role = new Role
		{
			Id = "role-id",
			Name = "admin",
			Description = "Administrator role",
			CreatedTimestamp = DateTimeOffset.UtcNow,
			ModifiedTimestamp = DateTimeOffset.UtcNow
		};

		// Act
		var json = JsonSerializer.Serialize(role);
		var deserialized = JsonSerializer.Deserialize<Role>(json);

		// Assert
		deserialized.Should().NotBeNull();
		deserialized!.Id.Should().Be(role.Id);
		deserialized.Name.Should().Be(role.Name);
		deserialized.Description.Should().Be(role.Description);
	}

	[Fact]
	public void Permission_SerializesAndDeserializes()
	{
		// Arrange
		var permission = new Permission
		{
			Id = "perm-id",
			Aco = "Resource",
			AcoForeignKey = "resource-id",
			Aro = "User",
			AroForeignKey = "user-id",
			Type = 7,
			CreatedTimestamp = DateTimeOffset.UtcNow,
			ModifiedTimestamp = DateTimeOffset.UtcNow
		};

		// Act
		var json = JsonSerializer.Serialize(permission);
		var deserialized = JsonSerializer.Deserialize<Permission>(json);

		// Assert
		deserialized.Should().NotBeNull();
		deserialized!.Aco.Should().Be("Resource");
		deserialized.Type.Should().Be(7);
	}

	[Fact]
	public void CreateCommentRequest_SerializesCorrectly()
	{
		// Arrange
		var request = new CreateCommentRequest
		{
			ResourceId = "resource-id",
			Content = "New comment"
		};

		// Act
		var json = JsonSerializer.Serialize(request);

		// Assert
		json.Should().Contain("resource-id");
		json.Should().Contain("New comment");
	}

	[Fact]
	public void IdentifiedItem_HandlesNullableTimestamps()
	{
		// Arrange
		var role = new Role
		{
			Id = "role-id",
			Name = "user",
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
}
