using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using App.Core.Todo;
using Moq;
using Xunit;

namespace App.Core.Tests;

public class AppCoreTests
{
    [Fact]
    public void UseAppCore_RegistersTodoServiceDescriptor()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();

        // Act
        var result = builder.UseAppCore();

        // Assert — verify the service types are registered as descriptors
        Assert.Same(builder, result);
        var todoDescriptor = builder.Services.FirstOrDefault(d => d.ServiceType == typeof(TodoService));
        var pokemonDescriptor = builder.Services.FirstOrDefault(d => d.ServiceType == typeof(App.Core.Poke.PokemonService));
        Assert.NotNull(todoDescriptor);
        Assert.NotNull(pokemonDescriptor);
    }

    [Fact]
    public void UseAppCore_TodoService_CanBeResolvedWithRepository()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();
        builder.UseAppCore();
        // Provide the required ITodoRepository dependency
        var repoMock = new Mock<ITodoRepository>();
        builder.Services.AddScoped(_ => repoMock.Object);

        // Act
        var provider = builder.Services.BuildServiceProvider();
        var service = provider.GetService<TodoService>();

        // Assert
        Assert.NotNull(service);
    }

    [Fact]
    public void UseAppCore_WithNullBuilder_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            ((WebApplicationBuilder)null!).UseAppCore()
        );
    }
}
