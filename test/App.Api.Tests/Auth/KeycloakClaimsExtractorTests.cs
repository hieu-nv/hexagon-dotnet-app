using System.Security.Claims;
using System.Text.Json;
using App.Api.Auth;

namespace App.Api.Tests.Auth;

public class KeycloakClaimsExtractorTests
{
    private readonly KeycloakClaimsExtractor _extractor = new();

    [Fact]
    public void IsValidPrincipal_ValidClaims_ReturnsTrue()
    {
        // Arrange
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "12345"),
            new Claim(ClaimTypes.Email, "test@example.com")
        };
        var identity = new ClaimsIdentity(claims, "TestAuthType");
        var principal = new ClaimsPrincipal(identity);

        // Act
        var result = _extractor.IsValidPrincipal(principal);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsValidPrincipal_MissingEmail_ReturnsFalse()
    {
        // Arrange
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "12345")
        };
        var identity = new ClaimsIdentity(claims, "TestAuthType");
        var principal = new ClaimsPrincipal(identity);

        // Act
        var result = _extractor.IsValidPrincipal(principal);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsValidPrincipal_Unauthenticated_ReturnsFalse()
    {
        // Arrange
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "12345"),
            new Claim(ClaimTypes.Email, "test@example.com")
        };
        var identity = new ClaimsIdentity(claims); // No authentication type
        var principal = new ClaimsPrincipal(identity);

        // Act
        var result = _extractor.IsValidPrincipal(principal);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void ExtractFromPrincipal_ValidPrincipal_ExtractsAllClaims()
    {
        // Arrange
        var realmAccess = new { roles = new[] { "admin", "user" } };
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "12345"),
            new Claim(ClaimTypes.Email, "admin@example.com"),
            new Claim("name", "Admin User"),
            new Claim("realm_access", JsonSerializer.Serialize(realmAccess)),
            new Claim("custom_department", "Engineering")
        };
        var identity = new ClaimsIdentity(claims, "Bearer");
        var principal = new ClaimsPrincipal(identity);

        // Act
        var user = _extractor.ExtractFromPrincipal(principal);

        // Assert
        Assert.NotNull(user);
        Assert.Equal("12345", user.Id);
        Assert.Equal("admin@example.com", user.Email);
        Assert.Equal("Admin User", user.Name);

        Assert.Contains("admin", user.Roles);
        Assert.Contains("user", user.Roles);
        Assert.Equal(2, user.Roles.Count);

        Assert.True(user.CustomClaims.ContainsKey("custom_department"));
        Assert.Equal("Engineering", user.CustomClaims["custom_department"]);
    }

    [Fact]
    public void ExtractFromPrincipal_StandardRoleClaims_ExtractsRoles()
    {
        // Arrange
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "12345"),
            new Claim(ClaimTypes.Email, "user@example.com"),
            new Claim(ClaimTypes.Role, "manager"),
            new Claim(ClaimTypes.Role, "employee")
        };
        var identity = new ClaimsIdentity(claims, "Bearer");
        var principal = new ClaimsPrincipal(identity);

        // Act
        var user = _extractor.ExtractFromPrincipal(principal);

        // Assert
        Assert.NotNull(user);
        Assert.Contains("manager", user.Roles);
        Assert.Contains("employee", user.Roles);
        Assert.Equal(2, user.Roles.Count);
    }

    [Fact]
    public void ExtractFromPrincipal_InvalidPrincipal_ReturnsNull()
    {
        // Arrange
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "12345") // Missing email
        };
        var identity = new ClaimsIdentity(claims, "Bearer");
        var principal = new ClaimsPrincipal(identity);

        // Act
        var user = _extractor.ExtractFromPrincipal(principal);

        // Assert
        Assert.Null(user);
    }
}
