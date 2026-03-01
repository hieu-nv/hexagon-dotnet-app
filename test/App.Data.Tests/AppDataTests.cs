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
}
