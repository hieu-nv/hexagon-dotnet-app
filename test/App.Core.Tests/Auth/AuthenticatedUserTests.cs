using App.Core.Auth;
using Xunit;

namespace App.Core.Tests.Auth;

public class AuthenticatedUserTests
{
    [Fact]
    public void HasRole_WithMatchingRole_ReturnsTrue()
    {
        // Arrange
        var user = new AuthenticatedUser
        {
            Id = "user-id",
            Email = "user@example.com",
            Roles = ["admin", "user"],
        };

        // Act
        var result = user.HasRole("admin");

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void HasRole_WithNoMatchingRole_ReturnsFalse()
    {
        // Arrange
        var user = new AuthenticatedUser
        {
            Id = "user-id",
            Email = "user@example.com",
            Roles = ["user"],
        };

        // Act
        var result = user.HasRole("admin");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void HasRole_IsCaseInsensitive()
    {
        // Arrange
        var user = new AuthenticatedUser
        {
            Id = "user-id",
            Email = "user@example.com",
            Roles = ["admin"],
        };

        // Act
        var resultLower = user.HasRole("admin");
        var resultUpper = user.HasRole("ADMIN");
        var resultMixed = user.HasRole("AdMin");

        // Assert
        Assert.True(resultLower);
        Assert.True(resultUpper);
        Assert.True(resultMixed);
    }

    [Fact]
    public void HasAnyRole_WithMatchingRole_ReturnsTrue()
    {
        // Arrange
        var user = new AuthenticatedUser
        {
            Id = "user-id",
            Email = "user@example.com",
            Roles = ["user"],
        };

        // Act
        var result = user.HasAnyRole("admin", "user");

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void HasAnyRole_WithNoMatchingRoles_ReturnsFalse()
    {
        // Arrange
        var user = new AuthenticatedUser
        {
            Id = "user-id",
            Email = "user@example.com",
            Roles = ["guest"],
        };

        // Act
        var result = user.HasAnyRole("admin", "user");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void HasAllRoles_WithAllMatchingRoles_ReturnsTrue()
    {
        // Arrange
        var user = new AuthenticatedUser
        {
            Id = "user-id",
            Email = "user@example.com",
            Roles = ["admin", "user", "moderator"],
        };

        // Act
        var result = user.HasAllRoles("admin", "user");

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void HasAllRoles_WithMissingRole_ReturnsFalse()
    {
        // Arrange
        var user = new AuthenticatedUser
        {
            Id = "user-id",
            Email = "user@example.com",
            Roles = ["user"],
        };

        // Act
        var result = user.HasAllRoles("admin", "user");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Constructor_WithRequiredProperties_CreatesSuccessfully()
    {
        // Act
        var user = new AuthenticatedUser
        {
            Id = "user-id",
            Email = "user@example.com",
            Name = "John Doe",
            Roles = ["admin"],
            CustomClaims = new Dictionary<string, string> { { "key", "value" } },
        };

        // Assert
        Assert.Equal("user-id", user.Id);
        Assert.Equal("user@example.com", user.Email);
        Assert.Equal("John Doe", user.Name);
        Assert.Single(user.Roles);
        Assert.Single(user.CustomClaims);
    }
}
