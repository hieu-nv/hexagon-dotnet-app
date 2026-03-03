using App.Api.Middleware;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;

namespace App.Api.Tests.Middleware;

public class SecurityHeadersMiddlewareTests
{
    private readonly SecurityHeadersMiddleware _middleware = new();

    [Fact]
    public async Task InvokeAsync_ShouldAddAllFiveSecurityHeaders()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var nextCalled = false;
        RequestDelegate next = _ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };

        // Act
        await _middleware.InvokeAsync(context, next);

        // Assert — all five required security headers must be present
        Assert.Equal("nosniff", context.Response.Headers["X-Content-Type-Options"].ToString());
        Assert.Equal("DENY", context.Response.Headers["X-Frame-Options"].ToString());
        Assert.Equal("1; mode=block", context.Response.Headers["X-XSS-Protection"].ToString());
        Assert.Equal("strict-origin-when-cross-origin", context.Response.Headers["Referrer-Policy"].ToString());
        Assert.Equal("default-src 'self'", context.Response.Headers["Content-Security-Policy"].ToString());
        Assert.True(nextCalled, "Next middleware delegate was not called.");
    }

    [Fact]
    public async Task InvokeAsync_ShouldCallNextMiddleware()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var nextCallCount = 0;
        RequestDelegate next = _ =>
        {
            nextCallCount++;
            return Task.CompletedTask;
        };

        // Act
        await _middleware.InvokeAsync(context, next);

        // Assert
        Assert.Equal(1, nextCallCount);
    }

    [Fact]
    public async Task InvokeAsync_WithNullContext_ThrowsArgumentNullException()
    {
        // Arrange
        RequestDelegate next = _ => Task.CompletedTask;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => _middleware.InvokeAsync(null!, next)
        );
    }

    [Fact]
    public async Task InvokeAsync_WithNullNext_ThrowsArgumentNullException()
    {
        // Arrange
        var context = new DefaultHttpContext();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => _middleware.InvokeAsync(context, null!)
        );
    }
}
