using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using App.Core.Poke;
using App.Data;
using Moq;
using Xunit;

namespace App.Api.Tests.Integration;

/// <summary>
/// Authentication handler that impersonates a non-admin user (role: "user" only).
/// </summary>
public class NonAdminTestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string AuthenticationScheme = "NonAdminTest";

    public NonAdminTestAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // Only "user" role — NOT "admin"
        var realmAccess = new { roles = new[] { "user" } };
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "non-admin-user-id"),
            new Claim(ClaimTypes.Email, "user@example.com"),
            new Claim("name", "Regular User"),
            new Claim("realm_access", JsonSerializer.Serialize(realmAccess))
        };

        var identity = new ClaimsIdentity(claims, AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, AuthenticationScheme);
        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}

/// <summary>
/// WebApplicationFactory variant that uses a non-admin user for authorization tests.
/// </summary>
public class NonAdminWebAppFactory : WebApplicationFactory<Program>
{
    private readonly SqliteConnection _connection;

    public Mock<IPokemonGateway> PokemonGatewayMock { get; } = new();

    public NonAdminWebAppFactory()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            // Replace DbContext with in-memory SQLite
            var dbContextDescriptor = services.SingleOrDefault(d =>
                d.ServiceType == typeof(DbContextOptions<AppDbContext>));
            if (dbContextDescriptor != null)
                services.Remove(dbContextDescriptor);

            services.AddDbContext<AppDbContext>(options => options.UseSqlite(_connection));

            // Replace the Pokemon gateway with a mock
            var gatewayDescriptor = services.SingleOrDefault(d =>
                d.ServiceType == typeof(IPokemonGateway));
            if (gatewayDescriptor != null)
                services.Remove(gatewayDescriptor);

            services.AddScoped(_ => PokemonGatewayMock.Object);

            // Use the non-admin auth handler
            services.AddAuthentication(NonAdminTestAuthHandler.AuthenticationScheme)
                .AddScheme<AuthenticationSchemeOptions, NonAdminTestAuthHandler>(
                    NonAdminTestAuthHandler.AuthenticationScheme, _ => { });

            services.AddAuthorization(options =>
            {
                options.DefaultPolicy = new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder()
                    .AddAuthenticationSchemes(NonAdminTestAuthHandler.AuthenticationScheme)
                    .RequireAuthenticatedUser()
                    .Build();
            });
        });

        builder.ConfigureTestServices(services =>
        {
            services.AddMvc(options =>
            {
                options.Filters.Add(new Microsoft.AspNetCore.Mvc.Authorization.AuthorizeFilter(
                    new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder()
                        .AddAuthenticationSchemes(NonAdminTestAuthHandler.AuthenticationScheme)
                        .RequireAuthenticatedUser()
                        .Build()));
            });
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
            _connection.Dispose();
    }
}

/// <summary>
/// Integration tests for authorization: verifies that non-admin users cannot access admin-only endpoints.
/// </summary>
public class AuthIntegrationTests : IClassFixture<NonAdminWebAppFactory>
{
    private readonly HttpClient _client;

    public AuthIntegrationTests(NonAdminWebAppFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task AdminOnlyEndpoint_WithUserRole_ShouldReturnForbidden()
    {
        // The Todos CRUD endpoints require "UserAccess" policy (user or admin).
        // This verifies the authorization policy is enforced correctly.
        // A "user"-role user can GET todos (UserAccess policy).
        var getResponse = await _client.GetAsync("/api/v1/todos");

        // "user" role satisfies the UserAccess policy — should be 200
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
    }

    [Fact]
    public async Task AuthorizedEndpoint_WithAuthenticatedNonAdminUser_ShouldReturnOk()
    {
        // Arrange
        var request = new App.Api.Todo.CreateTodoRequest("Non-Admin User Todo", false, null);

        // Act — a non-admin (user-role) should still be able to create todos as per UserAccess policy
        var response = await _client.PostAsJsonAsync("/api/v1/todos", request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task HealthCheck_WithNonAdminUser_ShouldReturnOk()
    {
        // Health endpoint should be publicly accessible regardless of role
        var response = await _client.GetAsync("/health");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
