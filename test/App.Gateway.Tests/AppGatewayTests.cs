using App.Gateway.Client;
using App.Gateway.Pokemon;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace App.Gateway.Tests;

public class AppGatewayTests
{
    [Fact]
    public void UseAppGateway_RegistersPokemonGateway()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();

        // Act
        var result = builder.UseAppGateway();

        // Assert
        Assert.Same(builder, result);
        var provider = builder.Services.BuildServiceProvider();
        Assert.NotNull(provider.GetService<App.Core.Pokemon.IPokemonGateway>());
    }

    [Fact]
    public void UseAppGateway_WithNullBuilder_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            ((WebApplicationBuilder)null!).UseAppGateway()
        );
    }
}

public class PokeClientConstructorTests
{
    [Fact]
    public void PokeClient_WithNullHttpClient_ShouldThrow()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new PokeClient(null!));
    }
}
