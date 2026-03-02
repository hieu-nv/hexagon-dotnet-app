using System.Security.Claims;
using App.Api.Auth;

namespace App.Api.Tests.Auth;

public class AuthServiceTests
{
    private readonly KeycloakClaimsExtractor _extractor = new();
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        _authService = new AuthService(_extractor);
    }

    [Fact]
    public void IsAuthenticated_ValidPrincipal_ReturnsTrue()
    {
        // Arrange
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "12345"),
            new Claim(ClaimTypes.Email, "test@example.com")
        };
        var identity = new ClaimsIdentity(claims, "Bearer");
        var principal = new ClaimsPrincipal(identity);

        // Act
        var result = _authService.IsAuthenticated(principal);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void AuthorizeUser_RequiredRolePresent_ReturnsTrue()
    {
        // Arrange
        var user = new AuthenticatedUser(
            "123",
            "test@example.com",
            "Test",
            new[] { "admin", "user" },
            new Dictionary<string, string>()
        );
        var policy = new AuthenticationPolicy("AdminOnly", null, new[] { "admin" });

        // Act
        var result = _authService.AuthorizeUser(user, policy);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void AuthorizeUser_RequiredRoleMissing_ReturnsFalse()
    {
        // Arrange
        var user = new AuthenticatedUser(
            "123",
            "test@example.com",
            "Test",
            new[] { "user" }, // Missing admin
            new Dictionary<string, string>()
        );
        var policy = new AuthenticationPolicy("AdminOnly", null, new[] { "admin" });

        // Act
        var result = _authService.AuthorizeUser(user, policy);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void AuthorizeUser_NoRequiredRoles_ReturnsTrue()
    {
        // Arrange
        var user = new AuthenticatedUser(
            "123",
            "test@example.com",
            "Test",
            new[] { "user" },
            new Dictionary<string, string>()
        );
        var policy = new AuthenticationPolicy("AnyAuthenticated", null, Array.Empty<string>());

        // Act
        var result = _authService.AuthorizeUser(user, policy);

        // Assert
        Assert.True(result); // Any authenticated user passes an empty roles policy
    }

    [Fact]
    public void AuthorizeUser_MultiplePoliciesRequiredOneMatches_ReturnsTrue()
    {
        // Arrange
        var user = new AuthenticatedUser(
            "123",
            "test@example.com",
            "Test",
            new[] { "moderator" },
            new Dictionary<string, string>()
        );
        var policy = new AuthenticationPolicy("AdminOrMod", null, new[] { "admin", "moderator" });

        // Act
        var result = _authService.AuthorizeUser(user, policy);

        // Assert
        Assert.True(result);
    }
}
