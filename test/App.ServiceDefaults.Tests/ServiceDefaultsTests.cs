using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Xunit;

namespace App.ServiceDefaults.Tests;

public class ServiceDefaultsTests
{
    [Fact]
    public void AddServiceDefaults_Succeeds()
    {
        // Arrange
        var builder = Host.CreateApplicationBuilder();

        // Act
        builder.AddServiceDefaults();

        // Assert
        var serviceProvider = builder.Services.BuildServiceProvider();
        Assert.NotNull(serviceProvider.GetService<IConfiguration>());
    }

    [Fact]
    public void ConfigureOpenTelemetry_WithDefaultConfig_Succeeds()
    {
        // Arrange
        var builder = Host.CreateApplicationBuilder();

        // Act
        builder.ConfigureOpenTelemetry();

        // Assert
        var serviceProvider = builder.Services.BuildServiceProvider();
        Assert.NotNull(serviceProvider);
    }

    [Fact]
    public void ConfigureOpenTelemetry_WithOtlpExporter_Succeeds()
    {
        // Arrange
        var builder = Host.CreateApplicationBuilder();
        var config = new Dictionary<string, string?>
        {
            ["OTEL_EXPORTER_OTLP_ENDPOINT"] = "http://localhost:4317",
            ["OTEL_SERVICE_NAME"] = "test-service"
        };
        builder.Configuration.AddInMemoryCollection(config);

        // Act
        builder.ConfigureOpenTelemetry();

        // Assert
        var serviceProvider = builder.Services.BuildServiceProvider();
        Assert.NotNull(serviceProvider);
    }

    [Fact]
    public void ConfigureOpenTelemetry_WithCloudOtlpExporter_Succeeds()
    {
        // Arrange
        var builder = Host.CreateApplicationBuilder();
        var config = new Dictionary<string, string?>
        {
            ["OTEL_EXPORTER_OTLP_ENDPOINT"] = "https://otlp.datadoghq.com",
            ["OTEL_SERVICE_NAME"] = "test-service"
        };
        builder.Configuration.AddInMemoryCollection(config);

        // Act
        builder.ConfigureOpenTelemetry();

        // Assert
        var serviceProvider = builder.Services.BuildServiceProvider();
        Assert.NotNull(serviceProvider);
    }

    [Fact]
    public void AddDefaultHealthChecks_Succeeds()
    {
        // Arrange
        var builder = Host.CreateApplicationBuilder();

        // Act
        builder.AddDefaultHealthChecks();

        // Assert
        var serviceProvider = builder.Services.BuildServiceProvider();
        Assert.NotNull(serviceProvider);
    }

    [Fact]
    public void AddServiceDefaults_WithStatsd_Succeeds()
    {
        // Arrange
        var builder = Host.CreateApplicationBuilder();
        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["METRICS_EXPORTER"] = "statsd",
            ["DD_AGENT_HOST"] = "localhost",
            ["DD_DOGSTATSD_PORT"] = "8125"
        });

        // Act
        builder.AddServiceDefaults();

        // Assert
        var serviceProvider = builder.Services.BuildServiceProvider();
        Assert.NotNull(serviceProvider);
    }

    [Fact]
    public void AddServiceDefaults_WithMetricsDisabled_Succeeds()
    {
        // Arrange
        var builder = Host.CreateApplicationBuilder();
        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["METRICS_EXPORTER"] = "disabled"
        });

        // Act
        builder.AddServiceDefaults();

        // Assert
        var serviceProvider = builder.Services.BuildServiceProvider();
        Assert.NotNull(serviceProvider);
    }

    [Fact]
    public void MapDefaultEndpoints_Succeeds()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();
        builder.AddServiceDefaults();
        var app = builder.Build();

        // Act
        app.MapDefaultEndpoints();

        // Assert
        Assert.NotNull(app);
    }
}
