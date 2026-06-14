namespace Passbolt.Api.Test;

/// <summary>
/// Unit tests for request and response contract initialization and serialization.
/// </summary>
public sealed class RequestResponseContractTests
{
	/// <summary>
	/// Verifies that CreateUserRequest initializes with required properties.
	/// </summary>
	[Fact]
	public void CreateUserRequest_RequiredProperties()
	{
		// Arrange & Act
		var request = new CreateUserRequest
		{
			Username = "newuser@example.com",
			FirstName = "John",
			LastName = "Doe"
		};

		// Assert
		request.Username.Should().Be("newuser@example.com");
		request.FirstName.Should().Be("John");
		request.LastName.Should().Be("Doe");
	}

	/// <summary>
	/// Verifies that UpdateUserRequest allows optional properties.
	/// </summary>
	[Fact]
	public void UpdateUserRequest_OptionalProperties()
	{
		// Arrange & Act
		var request = new UpdateUserRequest
		{
			FirstName = "Jane",
			LastName = "Smith"
		};

		// Assert
		request.FirstName.Should().Be("Jane");
		request.LastName.Should().Be("Smith");
	}

	/// <summary>
	/// Verifies that CreateResourceRequest initializes with required properties.
	/// </summary>
	[Fact]
	public void CreateResourceRequest_RequiredProperties()
	{
		// Arrange & Act
		var request = new CreateResourceRequest
		{
			Name = "My Password",
			Secret = "password123",
			Username = "user@example.com"
		};

		// Assert
		request.Name.Should().Be("My Password");
		request.Secret.Should().Be("password123");
		request.Username.Should().Be("user@example.com");
	}

	/// <summary>
	/// Verifies that UpdateResourceRequest allows property updates.
	/// </summary>
	[Fact]
	public void UpdateResourceRequest_Properties()
	{
		// Arrange & Act
		var request = new UpdateResourceRequest
		{
			Name = "Updated",
			Secret = "newpass"
		};

		// Assert
		request.Name.Should().Be("Updated");
		request.Secret.Should().Be("newpass");
	}

	/// <summary>
	/// Verifies that ShareResourceRequest accepts multiple permissions.
	/// </summary>
	[Fact]
	public void ShareResourceRequest_WithPermissions()
	{
		// Arrange
		var permissions = new List<SharePermissionRequest>
		{
			new() { UserId = "user-1", Type = 7 },
			new() { UserId = "user-2", Type = 1 }
		};

		// Act
		var request = new ShareResourceRequest { Permissions = permissions };

		// Assert
		request.Permissions.Should().HaveCount(2);
		request.Permissions[0].Type.Should().Be(7);
	}

	/// <summary>
	/// Verifies that CreateGroupRequest initializes with required properties.
	/// </summary>
	[Fact]
	public void CreateGroupRequest_Properties()
	{
		// Arrange & Act
		var request = new CreateGroupRequest { Name = "Engineering" };

		// Assert
		request.Name.Should().Be("Engineering");
	}

	/// <summary>
	/// Verifies that UpdateGroupRequest allows property updates.
	/// </summary>
	[Fact]
	public void UpdateGroupRequest_Properties()
	{
		// Arrange & Act
		var request = new UpdateGroupRequest { Name = "Updated Team" };

		// Assert
		request.Name.Should().Be("Updated Team");
	}

	/// <summary>
	/// Verifies that CreateFolderRequest initializes with required properties.
	/// </summary>
	[Fact]
	public void CreateFolderRequest_Properties()
	{
		// Arrange & Act
		var request = new CreateFolderRequest { Name = "My Folder" };

		// Assert
		request.Name.Should().Be("My Folder");
	}

	/// <summary>
	/// Verifies that UpdateFolderRequest allows property updates.
	/// </summary>
	[Fact]
	public void UpdateFolderRequest_Properties()
	{
		// Arrange & Act
		var request = new UpdateFolderRequest { Name = "Renamed" };

		// Assert
		request.Name.Should().Be("Renamed");
	}

	/// <summary>
	/// Verifies that SharePermissionRequest initializes correctly.
	/// </summary>
	[Fact]
	public void SharePermissionRequest_Properties()
	{
		// Arrange & Act
		var permission = new SharePermissionRequest { UserId = "user-1", Type = 7 };

		// Assert
		permission.UserId.Should().Be("user-1");
		permission.Type.Should().Be(7);
	}

	/// <summary>
	/// Verifies that GroupUserMembershipRequest initializes correctly.
	/// </summary>
	[Fact]
	public void GroupUserMembershipRequest_Properties()
	{
		// Arrange & Act
		var membership = new GroupUserMembershipRequest { UserId = "user-1", IsAdmin = true };

		// Assert
		membership.UserId.Should().Be("user-1");
		membership.IsAdmin.Should().BeTrue();
	}
}
