using Serilog;
using Serilog.Events;
using Serilog.Formatting.Json;
using Serilog.Sinks.Datadog.Logs;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Configure Serilog with multiple sinks
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Application", "App.Api")
    .Enrich.WithProperty("Environment", builder.Environment.EnvironmentName)
    .Enrich.WithProperty("service", "hexagon-dotnet-app")
    .WriteTo.Console()
    .WriteTo.File(
        new JsonFormatter(),
        path: "logs/app.log",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 7,
        shared: true,
        flushToDiskInterval: TimeSpan.FromSeconds(1))
    .WriteTo.DatadogLogs(
        apiKey: "7330e4f63b184ae564bfcb8abe595196",
        source: "csharp",
        service: "hexagon-dotnet-app",
        host: Environment.MachineName,
        tags: new[] { $"env:{builder.Environment.EnvironmentName}", "version:1.0.0" },
        configuration: new DatadogConfiguration
        {
            Url = "https://http-intake.logs.us5.datadoghq.com"
        }
    )
    .CreateLogger();

builder.Host.UseSerilog();

// Add Aspire service defaults (OpenTelemetry, service discovery, resilience)
builder.AddServiceDefaults();

builder.UseAppCore();
builder.UseAppData();
builder.UseAppGateway();

builder.UseTodo();
builder.UsePokemon();

WebApplication app = builder.Build();
app.UseAppData();

// Map Aspire default endpoints (/health, /alive)
app.MapDefaultEndpoints();

if (app.Environment.IsDevelopment())
{
    _ = app.UseDeveloperExceptionPage();
}

app.UseTodo();
app.UsePokemon();

try
{
    Log.Information("Starting Hexagon .NET App");
    await app.RunAsync().ConfigureAwait(false);
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
    throw;
}
finally
{
    await Log.CloseAndFlushAsync().ConfigureAwait(false);
}
