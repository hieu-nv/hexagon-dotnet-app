using System.Security.Claims;
using App.Api.Auth;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace App.Api.Tests.Auth;

public class AuthInfoEndpointsTests
{
    private readonly ILogger<AuthInfoEndpoints> _logger;
    private readonly AuthService _authService;
    private readonly AuthInfoEndpoints _endpoints;

    private class FakeClaimsExtractor : IClaimsExtractor
    {
        public AuthenticatedUser? ExtractFromPrincipal(ClaimsPrincipal principal) => null;
        public bool IsValidPrincipal(ClaimsPrincipal principal) => false;
    }

    public AuthInfoEndpointsTests()
    {
        _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger<AuthInfoEndpoints>.Instance;
        _authService = new AuthService(new FakeClaimsExtractor());
        _endpoints = new AuthInfoEndpoints(_authService, _logger);
    }

    [Fact]
    public void GetCurrentUser_WithNullUser_ReturnsUnauthorized()
    {
        // Arrange
        var principal = new ClaimsPrincipal(); // Missing necessary claims

        // Act
        var result = _endpoints.GetCurrentUser(principal);

        // Assert
        Assert.NotNull(result);
        // We know it returns Results.Unauthorized()
        var unauthorizedResult = Assert.IsAssignableFrom<Microsoft.AspNetCore.Http.HttpResults.UnauthorizedHttpResult>(result);
        Assert.Equal(401, unauthorizedResult.StatusCode);
    }

    [Fact]
    public void Constructor_WithNullAuthService_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new AuthInfoEndpoints(null!, _logger));
    }

    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new AuthInfoEndpoints(_authService, null!));
    }
}
