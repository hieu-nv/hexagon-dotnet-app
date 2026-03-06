using System.Security.Claims;
using App.Api.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace App.Api.Tests.Auth;

public class AuthLogoutEndpointsTests
{
    private readonly Mock<IConfiguration> _configMock;
    private readonly ILogger<AuthLogoutEndpoints> _logger;
    private readonly AuthLogoutEndpoints _endpoints;

    public AuthLogoutEndpointsTests()
    {
        _configMock = new Mock<IConfiguration>();
        _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger<AuthLogoutEndpoints>.Instance;
        _endpoints = new AuthLogoutEndpoints(_configMock.Object, _logger);
    }

    [Fact]
    public async Task LogoutAsync_NotAuthenticated_ReturnsUnauthorized()
    {
        // Arrange
        var user = new ClaimsPrincipal(new ClaimsIdentity());
        var context = new DefaultHttpContext();

        // Act
        var result = await _endpoints.LogoutAsync(user, context);

        // Assert
        var unauthorizedResult = Assert.IsAssignableFrom<Microsoft.AspNetCore.Http.HttpResults.UnauthorizedHttpResult>(result);
        Assert.Equal(401, unauthorizedResult.StatusCode);
    }

    [Fact]
    public void Constructor_WithNullConfiguration_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new AuthLogoutEndpoints(null!, _logger));
    }

    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new AuthLogoutEndpoints(_configMock.Object, null!));
    }
}
