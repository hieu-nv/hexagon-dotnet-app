using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using App.Core.Todo;
using App.Data;
using App.Data.Todo;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace App.Data.Tests;

public class AppDataTests
{
    [Fact]
    public void UseAppData_Builder_RegistersServices()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();

        // Act
        builder.UseAppData();

        // Assert
        var serviceProvider = builder.Services.BuildServiceProvider();
        Assert.NotNull(serviceProvider.GetService<AppDbContext>());
        Assert.NotNull(serviceProvider.GetService<ITodoRepository>());
    }

    [Fact]
    public void UseAppData_App_EnsuresCreated()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseInMemoryDatabase("AppDataTests")
        );
        var app = builder.Build();

        // Act
        app.UseAppData();

        // Assert
        Assert.NotNull(app);
    }

    [Fact]
    public void UseAppData_Builder_WithNullBuilder_ShouldThrow()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            ((WebApplicationBuilder)null!).UseAppData()
        );
    }

    [Fact]
    public void UseAppData_App_WithNullApp_ShouldThrow()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            ((WebApplication)null!).UseAppData()
        );
    }

    [Fact]
    public void UseAppData_Builder_ReturnsBuilder()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();

        // Act
        var result = builder.UseAppData();

        // Assert
        Assert.Same(builder, result);
    }
}
