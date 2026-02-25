using App.Api.Logging;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Json;
using Serilog.Sinks.Datadog.Logs;

// Load environment variables from .env file
DotNetEnv.Env.Load();

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Configure Serilog with multiple sinks
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .Enrich.With(new OpenTelemetryTraceEnricher())
    .Enrich.WithProperty("Application", "App.Api")
    .Enrich.WithProperty("Environment", builder.Environment.EnvironmentName)
    .Enrich.WithProperty("service", "hexagon-dotnet-app")
    .WriteTo.Console(new JsonFormatter(renderMessage: true))
    .WriteTo.File(
        new JsonFormatter(renderMessage: true),
        path: "logs/app.log",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 7,
        shared: true,
        flushToDiskInterval: TimeSpan.FromSeconds(1)
    )
    // For local Datadog agent: logs are written to file and agent tails them
    // For production: uncomment DatadogLogs sink with cloud URL
    // .WriteTo.DatadogLogs(
    //     apiKey: builder.Configuration["DD_API_KEY"],
    //     source: "csharp",
    //     service: "hexagon-dotnet-app",
    //     host: Environment.MachineName,
    //     tags: new[] { $"env:{builder.Environment.EnvironmentName}", "version:1.0.0" },
    //     configuration: new DatadogConfiguration { Url = "https://http-intake.logs.us5.datadoghq.com" }
    // )
    .CreateLogger();

builder.Host.UseSerilog();

// Filter out verbose HTTP resilience logs (404s are expected for invalid Pokemon IDs)
builder.Logging.AddFilter("Microsoft.Extensions.Http.Resilience", LogLevel.Warning);

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
catch (Exception ex) when (LogFatalException(ex))
{
    // Exception is logged in the when clause and never enters this block
}
finally
{
    await Log.CloseAndFlushAsync().ConfigureAwait(false);
}

static bool LogFatalException(Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
    return false; // Ensures the exception continues to propagate
}
