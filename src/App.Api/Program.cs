using System.Threading.RateLimiting;
using App.Api.Logging;
using App.Api.Middleware;
using FluentValidation;
using Microsoft.AspNetCore.RateLimiting;
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

// FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

// Global exception handler with RFC 7807 ProblemDetails
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// Rate limiting: fixed window - 100 requests per minute
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddFixedWindowLimiter(
        "fixed",
        opt =>
        {
            opt.PermitLimit = 100;
            opt.Window = TimeSpan.FromMinutes(1);
            opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
            opt.QueueLimit = 5;
        }
    );
});

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy(
        "DefaultPolicy",
        policy =>
        {
            policy
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader();
        }
    );
});

// Output caching for Pokemon endpoints
builder.Services.AddOutputCache(options =>
{
    options.AddPolicy("PokemonCache", b => b.Expire(TimeSpan.FromMinutes(5)));
});

WebApplication app = builder.Build();
app.UseAppData();

// Map Aspire default endpoints (/health, /alive)
app.MapDefaultEndpoints();

if (app.Environment.IsDevelopment())
{
    _ = app.UseDeveloperExceptionPage();
}

// Middleware pipeline
app.UseExceptionHandler();
app.UseCors("DefaultPolicy");
app.UseRateLimiter();
app.UseOutputCache();

// Security headers middleware
app.Use(async (context, next) =>
{
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    context.Response.Headers["X-Frame-Options"] = "DENY";
    context.Response.Headers["X-XSS-Protection"] = "1; mode=block";
    context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
    context.Response.Headers["Content-Security-Policy"] = "default-src 'self'";
    await next().ConfigureAwait(false);
});

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
