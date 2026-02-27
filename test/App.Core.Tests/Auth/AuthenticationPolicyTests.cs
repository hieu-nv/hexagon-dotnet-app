using App.Core.Auth;
using Xunit;

namespace App.Core.Tests.Auth;

public class AuthenticationPolicyTests
{
    [Fact]
    public void IsSatisfiedBy_WhenNoRolesRequired_ReturnsTrue()
    {
        // Arrange
        var policy = new AuthenticationPolicy { Name = "Authenticated", RequiredRoles = [] };
        var user = new AuthenticatedUser
        {
            Id = "user-id",
            Email = "user@example.com",
            Roles = [],
        };

        // Act
        var result = policy.IsSatisfiedBy(user);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsSatisfiedBy_WhenUserHasRequiredRole_ReturnsTrue()
    {
        // Arrange
        var policy = new AuthenticationPolicy { Name = "AdminOnly", RequiredRoles = ["admin"] };
        var user = new AuthenticatedUser
        {
            Id = "user-id",
            Email = "user@example.com",
            Roles = ["admin", "user"],
        };

        // Act
        var result = policy.IsSatisfiedBy(user);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsSatisfiedBy_WhenUserMissingRequiredRole_ReturnsFalse()
    {
        // Arrange
        var policy = new AuthenticationPolicy { Name = "AdminOnly", RequiredRoles = ["admin"] };
        var user = new AuthenticatedUser
        {
            Id = "user-id",
            Email = "user@example.com",
            Roles = ["user"],
        };

        // Act
        var result = policy.IsSatisfiedBy(user);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsSatisfiedBy_WhenUserHasAnyRequiredRole_ReturnsTrue()
    {
        // Arrange
        var policy = new AuthenticationPolicy
        {
            Name = "RestrictedAccess",
            RequiredRoles = ["admin", "moderator"],
        };
        var user = new AuthenticatedUser
        {
            Id = "user-id",
            Email = "user@example.com",
            Roles = ["moderator", "user"],
        };

        // Act
        var result = policy.IsSatisfiedBy(user);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void AdminOnly_CreatesProperPolicy()
    {
        // Act
        var policy = AuthenticationPolicy.AdminOnly();

        // Assert
        Assert.Equal("AdminOnly", policy.Name);
        Assert.Single(policy.RequiredRoles);
        Assert.Contains("admin", policy.RequiredRoles);
    }

    [Fact]
    public void AuthenticatedOnly_CreatesProperPolicy()
    {
        // Act
        var policy = AuthenticationPolicy.AuthenticatedOnly();

        // Assert
        Assert.Equal("AuthenticatedOnly", policy.Name);
        Assert.Empty(policy.RequiredRoles);
    }
}
