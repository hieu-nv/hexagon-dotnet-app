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
    .CreateLogger();

builder.Host.UseSerilog();

// Filter out verbose HTTP resilience logs (404s are expected for invalid Pokemon IDs)
builder.Logging.AddFilter("Microsoft.Extensions.Http.Resilience", LogLevel.Warning);

// Add Aspire service defaults (OpenTelemetry, service discovery, resilience)
builder.AddServiceDefaults();

builder.UseAppCore();
builder.UseAppData();
builder.UseAppGateway();

builder
    .Services.AddApiVersioning(options =>
    {
        options.DefaultApiVersion = new Asp.Versioning.ApiVersion(1, 0);
        options.AssumeDefaultVersionWhenUnspecified = true;
        options.ReportApiVersions = true;
    })
    .AddApiExplorer(options =>
    {
        options.GroupNameFormat = "'v'VVV";
        options.SubstituteApiVersionInUrl = true;
    });

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

var apiVersionSet = app.NewApiVersionSet()
    .HasApiVersion(new Asp.Versioning.ApiVersion(1, 0))
    .ReportApiVersions()
    .Build();

app.UseTodo(apiVersionSet);
app.UsePokemon(apiVersionSet);

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
