using System.Threading.RateLimiting;

using App.Api.Auth;
using App.Api.Logging;
using App.Api.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;

using FluentValidation;

using Microsoft.AspNetCore.RateLimiting;

using Serilog;
using Serilog.Events;
using Serilog.Formatting.Json;

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

// Configure Authentication
var jwtEnabled = builder.Configuration.GetValue<bool>("JwtBearer:Enabled");
if (jwtEnabled)
{
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.Authority = builder.Configuration["JwtBearer:Authority"];
            options.Audience = builder.Configuration["JwtBearer:Audience"];
            options.RequireHttpsMetadata = builder.Configuration.GetValue<bool>("JwtBearer:RequireHttpsMetadata");
            // Optional: Map inbound claims to custom types if needed
            // options.TokenValidationParameters.NameClaimType = "preferred_username";
            // options.TokenValidationParameters.RoleClaimType = "realm_access.roles"; // This often requires custom mapping logic
        });

    builder.Services.AddAuthorization(options =>
    {
        options.AddPolicy(AuthorizationPolicies.AdminOnly, policy => policy.RequireAssertion(context =>
        {
            if (context.Resource is HttpContext httpContext)
            {
                var claimsExtractor = httpContext.RequestServices.GetRequiredService<IClaimsExtractor>();
                var authService = httpContext.RequestServices.GetRequiredService<AuthService>();
                var user = claimsExtractor.ExtractFromPrincipal(context.User);
                return user != null && authService.AuthorizeByRoles(user, new[] { "admin" });
            }
            return false;
        }));

        options.AddPolicy(AuthorizationPolicies.UserAccess, policy => policy.RequireAssertion(context =>
        {
            if (context.Resource is HttpContext httpContext)
            {
                var claimsExtractor = httpContext.RequestServices.GetRequiredService<IClaimsExtractor>();
                var authService = httpContext.RequestServices.GetRequiredService<AuthService>();
                var user = claimsExtractor.ExtractFromPrincipal(context.User);
                return user != null && authService.AuthorizeByRoles(user, new[] { "user", "admin" });
            }
            return false;
        }));
    });

    // Register domain services
    builder.Services.AddScoped<IClaimsExtractor, KeycloakClaimsExtractor>();
    builder.Services.AddScoped<AuthService>();
}

// FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

// Global exception handler with RFC 7807 ProblemDetails
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.AddTransient<SecurityHeadersMiddleware>();

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

// CORS — config-driven origins
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>();
builder.Services.AddCors(options =>
{
    options.AddPolicy(
        "DefaultPolicy",
        policy =>
        {
            if (allowedOrigins is { Length: > 0 })
            {
                policy
                    .WithOrigins(allowedOrigins)
                    .AllowAnyMethod()
                    .AllowAnyHeader();
            }
            else if (builder.Environment.IsProduction())
            {
                // Fail fast in Production to prevent accidentally open CORS
                throw new InvalidOperationException(
                    "Cors:AllowedOrigins must be configured in Production environments. " +
                    "Set at least one allowed origin in appsettings or environment variables."
                );
            }
            else
            {
                // Fallback for Development and Testing environments when no origins are configured
                policy
                    .AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader();
            }
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

if (jwtEnabled)
{
    app.UseAuthentication();
    app.UseAuthorization();
}

// Security headers middleware
app.UseMiddleware<SecurityHeadersMiddleware>();

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
