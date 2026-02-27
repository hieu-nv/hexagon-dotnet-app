using App.Api.Auth;
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

// Configure authorization policies for SAML authentication
if (builder.Configuration.GetValue<bool>("Saml2:Enabled", false))
{
    builder.Services.AddSaml2AuthorizationPolicies();
}

// Add Aspire service defaults (OpenTelemetry, service discovery, resilience)
builder.AddServiceDefaults();

// Configure SAML2 authentication (only if enabled in configuration)
if (builder.Configuration.GetValue<bool>("Saml2:Enabled", false))
{
    builder.AddSaml2Authentication();
}

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

// Use SAML authentication if enabled
if (app.Configuration.GetValue<bool>("Saml2:Enabled", false))
{
    app.UseAuthentication();
    app.UseAuthorization();
    app.UseSaml2();
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
