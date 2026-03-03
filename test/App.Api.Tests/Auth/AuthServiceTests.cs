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
        var result = AuthService.AuthorizeUser(user, policy);

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
        var result = AuthService.AuthorizeUser(user, policy);

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
        var result = AuthService.AuthorizeUser(user, policy);

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
        var result = AuthService.AuthorizeUser(user, policy);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Constructor_WithNullExtractor_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new AuthService(null!));
    }

    [Fact]
    public void GetAuthenticatedUser_ValidPrincipal_ReturnsUser()
    {
        // Arrange
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "42"),
            new Claim(ClaimTypes.Email, "user@example.com"),
            new Claim("name", "Test User")
        };
        var identity = new ClaimsIdentity(claims, "Bearer");
        var principal = new ClaimsPrincipal(identity);

        // Act
        var result = _authService.GetAuthenticatedUser(principal);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("42", result.Id);
        Assert.Equal("user@example.com", result.Email);
        Assert.Equal("Test User", result.Name);
    }

    [Fact]
    public void GetAuthenticatedUser_InvalidPrincipal_ReturnsNull()
    {
        // Arrange — unauthenticated identity (no auth type)
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "42"),
            new Claim(ClaimTypes.Email, "user@example.com")
        };
        var identity = new ClaimsIdentity(claims); // No authentication type → not authenticated
        var principal = new ClaimsPrincipal(identity);

        // Act
        var result = _authService.GetAuthenticatedUser(principal);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void AuthorizeByRoles_WithMatchingRole_ReturnsTrue()
    {
        // Arrange
        var user = new AuthenticatedUser(
            "1", "a@b.com", "A",
            new[] { "editor", "viewer" },
            new Dictionary<string, string>()
        );

        // Act
        var result = AuthService.AuthorizeByRoles(user, new[] { "editor" });

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void AuthorizeByRoles_WithNoMatchingRole_ReturnsFalse()
    {
        // Arrange
        var user = new AuthenticatedUser(
            "1", "a@b.com", "A",
            new[] { "viewer" },
            new Dictionary<string, string>()
        );

        // Act
        var result = AuthService.AuthorizeByRoles(user, new[] { "admin", "editor" });

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void AuthorizeByRoles_WithNullUser_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(
            () => AuthService.AuthorizeByRoles(null!, new[] { "admin" })
        );
    }

    [Fact]
    public void AuthorizeByRoles_WithNullRoles_ThrowsArgumentNullException()
    {
        // Arrange
        var user = new AuthenticatedUser(
            "1", "a@b.com", "A",
            new[] { "viewer" },
            new Dictionary<string, string>()
        );

        // Act & Assert
        Assert.Throws<ArgumentNullException>(
            () => AuthService.AuthorizeByRoles(user, null!)
        );
    }

    [Fact]
    public void AuthenticatedUser_HasRole_CaseInsensitive_ReturnsTrue()
    {
        // Arrange
        var user = new AuthenticatedUser(
            "1", "a@b.com", "A",
            new[] { "admin" },
            new Dictionary<string, string>()
        );

        // Act & Assert — "ADMIN" should match "admin"
        Assert.True(user.HasRole("ADMIN"));
        Assert.True(user.HasRole("Admin"));
    }

    [Fact]
    public void AuthenticatedUser_HasRole_NotPresent_ReturnsFalse()
    {
        // Arrange
        var user = new AuthenticatedUser(
            "1", "a@b.com", "A",
            new[] { "viewer" },
            new Dictionary<string, string>()
        );

        // Act & Assert
        Assert.False(user.HasRole("admin"));
    }
}
