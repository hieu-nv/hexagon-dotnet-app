using FluentValidation;
using FluentValidation.Results;
using App.Api.Filters;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;

namespace App.Api.Tests.Filters;

public class ValidationFilterTests
{
    public class TestRequest { public string Name { get; set; } = string.Empty; }

    [Fact]
    public async Task InvokeAsync_WithValidRequest_ShouldCallNext()
    {
        // Arrange
        var request = new TestRequest { Name = "Valid" };
        var validatorMock = new Mock<IValidator<TestRequest>>();
        validatorMock.Setup(v => v.ValidateAsync(request, default))
            .ReturnsAsync(new ValidationResult());

        var serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock.Setup(s => s.GetService(typeof(IValidator<TestRequest>)))
            .Returns(validatorMock.Object);

        var context = new DefaultHttpContext { RequestServices = serviceProviderMock.Object };
        var filterContext = new DefaultEndpointFilterInvocationContext(context, request);
        
        bool nextCalled = false;
        EndpointFilterDelegate next = (invocationContext) => {
            nextCalled = true;
            return ValueTask.FromResult<object?>(Results.Ok());
        };

        var filter = new ValidationFilter<TestRequest>();

        // Act
        await filter.InvokeAsync(filterContext, next);

        // Assert
        Assert.True(nextCalled);
    }

    [Fact]
    public async Task InvokeAsync_WithInvalidRequest_ShouldReturnValidationProblem()
    {
        // Arrange
        var request = new TestRequest { Name = "" };
        var validatorMock = new Mock<IValidator<TestRequest>>();
        validatorMock.Setup(v => v.ValidateAsync(request, default))
            .ReturnsAsync(new ValidationResult(new[] { new ValidationFailure("Name", "Name is required") }));

        var serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock.Setup(s => s.GetService(typeof(IValidator<TestRequest>)))
            .Returns(validatorMock.Object);

        var context = new DefaultHttpContext { RequestServices = serviceProviderMock.Object };
        var filterContext = new DefaultEndpointFilterInvocationContext(context, request);
        
        EndpointFilterDelegate next = (invocationContext) => ValueTask.FromResult<object?>(Results.Ok());

        var filter = new ValidationFilter<TestRequest>();

        // Act
        var result = await filter.InvokeAsync(filterContext, next);

        // Assert
        // We expect a ValidationProblem result (which implements IStatusCodeHttpResult and IValueHttpResult)
        var statusCodeResult = Assert.IsAssignableFrom<IStatusCodeHttpResult>(result);
        Assert.Equal(StatusCodes.Status400BadRequest, statusCodeResult.StatusCode);
    }

    private class DefaultEndpointFilterInvocationContext : EndpointFilterInvocationContext
    {
        public DefaultEndpointFilterInvocationContext(HttpContext httpContext, params object?[] arguments)
        {
            HttpContext = httpContext;
            Arguments = arguments;
        }

        public override HttpContext HttpContext { get; }
        public override IList<object?> Arguments { get; }
        public override T GetArgument<T>(int index) => (T)Arguments[index]!;
    }
}
