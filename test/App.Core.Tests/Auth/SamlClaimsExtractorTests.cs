using System.Security.Claims;
using App.Core.Auth;
using Xunit;

namespace App.Core.Tests.Auth;

public class SamlClaimsExtractorTests
{
    private readonly MockSamlClaimsExtractor _extractor = new();

    private sealed class MockSamlClaimsExtractor : ISamlClaimsExtractor
    {
        public AuthenticatedUser? ExtractFromPrincipal(ClaimsPrincipal claimsPrincipal)
        {
            ArgumentNullException.ThrowIfNull(claimsPrincipal);

            if (!claimsPrincipal.Identity?.IsAuthenticated ?? true)
            {
                return null;
            }

            var email = claimsPrincipal.FindFirst(ClaimTypes.Email)?.Value;
            var nameId = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(nameId))
            {
                return null;
            }

            var name = claimsPrincipal.FindFirst(ClaimTypes.Name)?.Value;
            var roles = claimsPrincipal
                .FindAll(ClaimTypes.Role)
                .Select(c => c.Value)
                .Where(v => !string.IsNullOrWhiteSpace(v))
                .ToList();

            var customClaims = claimsPrincipal
                .Claims.Where(c =>
                    !new[]
                    {
                        ClaimTypes.Email,
                        ClaimTypes.Name,
                        ClaimTypes.NameIdentifier,
                        ClaimTypes.Role,
                    }.Contains(c.Type, StringComparer.OrdinalIgnoreCase)
                )
                .DistinctBy(c => c.Type)
                .ToDictionary(c => c.Type, c => c.Value)
                .AsReadOnly();

            return new AuthenticatedUser
            {
                Id = nameId,
                Email = email,
                Name = name,
                Roles = roles,
                CustomClaims = customClaims,
            };
        }

        public bool IsValidSamlPrincipal(ClaimsPrincipal claimsPrincipal)
        {
            ArgumentNullException.ThrowIfNull(claimsPrincipal);

            if (!claimsPrincipal.Identity?.IsAuthenticated ?? true)
            {
                return false;
            }

            var hasEmail = claimsPrincipal.FindFirst(ClaimTypes.Email) is not null;
            var hasNameId = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier) is not null;

            return hasEmail && hasNameId;
        }
    }

    [Fact]
    public void IsValidSamlPrincipal_WhenPrincipalIsNull_ThrowsArgumentNullException()
    {
        // Arrange
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        ClaimsPrincipal nullPrincipal = null;
#pragma warning restore CS8625

        // Act & Assert
        _ = Assert.Throws<ArgumentNullException>(() =>
            _extractor.IsValidSamlPrincipal(nullPrincipal)
        );
    }

    [Fact]
    public void IsValidSamlPrincipal_WhenUnauthenticated_ReturnsFalse()
    {
        // Arrange
        var principal = new ClaimsPrincipal(new ClaimsIdentity());

        // Act
        var result = _extractor.IsValidSamlPrincipal(principal);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsValidSamlPrincipal_WhenMissingEmail_ReturnsFalse()
    {
        // Arrange
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, "user@example.com") };
        var identity = new ClaimsIdentity(claims, "SAML2");
        var principal = new ClaimsPrincipal(identity);

        // Act
        var result = _extractor.IsValidSamlPrincipal(principal);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsValidSamlPrincipal_WhenMissingNameId_ReturnsFalse()
    {
        // Arrange
        var claims = new[] { new Claim(ClaimTypes.Email, "user@example.com") };
        var identity = new ClaimsIdentity(claims, "SAML2");
        var principal = new ClaimsPrincipal(identity);

        // Act
        var result = _extractor.IsValidSamlPrincipal(principal);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsValidSamlPrincipal_WhenHasRequiredClaims_ReturnsTrue()
    {
        // Arrange
        var claims = new[]
        {
            new Claim(ClaimTypes.Email, "user@example.com"),
            new Claim(ClaimTypes.NameIdentifier, "user@example.com"),
        };
        var identity = new ClaimsIdentity(claims, "SAML2");
        var principal = new ClaimsPrincipal(identity);

        // Act
        var result = _extractor.IsValidSamlPrincipal(principal);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void ExtractFromPrincipal_WhenPrincipalIsNull_ThrowsArgumentNullException()
    {
        // Arrange
#pragma warning disable CS8625
        ClaimsPrincipal nullPrincipal = null;
#pragma warning restore CS8625

        // Act & Assert
        _ = Assert.Throws<ArgumentNullException>(() =>
            _extractor.ExtractFromPrincipal(nullPrincipal)
        );
    }

    [Fact]
    public void ExtractFromPrincipal_WhenInvalid_ReturnsNull()
    {
        // Arrange
        var principal = new ClaimsPrincipal(new ClaimsIdentity());

        // Act
        var result = _extractor.ExtractFromPrincipal(principal);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void ExtractFromPrincipal_WithMinimalClaims_ReturnsAuthenticatedUser()
    {
        // Arrange
        var claims = new[]
        {
            new Claim(ClaimTypes.Email, "user@example.com"),
            new Claim(ClaimTypes.NameIdentifier, "user-id"),
        };
        var identity = new ClaimsIdentity(claims, "SAML2");
        var principal = new ClaimsPrincipal(identity);

        // Act
        var result = _extractor.ExtractFromPrincipal(principal);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("user-id", result.Id);
        Assert.Equal("user@example.com", result.Email);
        Assert.Null(result.Name);
        Assert.Empty(result.Roles);
    }

    [Fact]
    public void ExtractFromPrincipal_WithAllClaims_ReturnsCompleteAuthenticatedUser()
    {
        // Arrange
        var claims = new[]
        {
            new Claim(ClaimTypes.Email, "user@example.com"),
            new Claim(ClaimTypes.NameIdentifier, "user-id"),
            new Claim(ClaimTypes.Name, "John Doe"),
            new Claim(ClaimTypes.Role, "admin"),
            new Claim(ClaimTypes.Role, "user"),
            new Claim("department", "engineering"),
        };
        var identity = new ClaimsIdentity(claims, "SAML2");
        var principal = new ClaimsPrincipal(identity);

        // Act
        var result = _extractor.ExtractFromPrincipal(principal);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("user-id", result.Id);
        Assert.Equal("user@example.com", result.Email);
        Assert.Equal("John Doe", result.Name);
        Assert.Equal(2, result.Roles.Count);
        Assert.Contains("admin", result.Roles);
        Assert.Contains("user", result.Roles);
        Assert.Single(result.CustomClaims);
        Assert.True(result.CustomClaims.ContainsKey("department"));
    }
}
