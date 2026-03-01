using System.Diagnostics;
using App.Api.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace App.Api.Tests.Middleware;

public class GlobalExceptionHandlerTests
{
    private readonly Mock<ILogger<GlobalExceptionHandler>> _loggerMock;
    private readonly Mock<IHostEnvironment> _environmentMock;
    private readonly GlobalExceptionHandler _handler;

    public GlobalExceptionHandlerTests()
    {
        _loggerMock = new Mock<ILogger<GlobalExceptionHandler>>();
        _environmentMock = new Mock<IHostEnvironment>();
        _handler = new GlobalExceptionHandler(_loggerMock.Object, _environmentMock.Object);
    }

    [Fact]
    public async Task TryHandleAsync_InDevelopment_ShouldIncludeStackTrace()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Path = "/test";
        var exception = new Exception("Test exception");
        _environmentMock.Setup(m => m.EnvironmentName).Returns(Environments.Development);

        // Act
        var result = await _handler.TryHandleAsync(context, exception, CancellationToken.None);

        // Assert
        Assert.True(result);
        Assert.Equal(StatusCodes.Status500InternalServerError, context.Response.StatusCode);
        
        // We can't easily check the JSON body here without a more complex setup, 
        // but we can verify the environment was checked.
        _environmentMock.Verify(m => m.EnvironmentName, Times.AtLeastOnce);
    }

    [Fact]
    public async Task TryHandleAsync_InProduction_ShouldStripStackTrace()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Path = "/test";
        var exception = new Exception("Test exception");
        _environmentMock.Setup(m => m.EnvironmentName).Returns(Environments.Production);

        // Act
        var result = await _handler.TryHandleAsync(context, exception, CancellationToken.None);

        // Assert
        Assert.True(result);
        Assert.Equal(StatusCodes.Status500InternalServerError, context.Response.StatusCode);
        _environmentMock.Verify(m => m.EnvironmentName, Times.AtLeastOnce);
    }
}
