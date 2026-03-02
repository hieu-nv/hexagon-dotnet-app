using App.Core.Poke;
using App.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace App.Api.Tests.Integration;

/// <summary>
/// Custom WebApplicationFactory for integration tests.
/// Replaces external dependencies (Pokemon gateway) with mocks
/// and uses a dedicated in-memory SQLite database per factory instance.
/// </summary>
public class IntegrationTestWebAppFactory : WebApplicationFactory<Program>
{
    // Keep the connection open for the lifetime of the factory so the
    // in-memory database is not destroyed between requests.
    private readonly SqliteConnection _connection;

    public Mock<IPokemonGateway> PokemonGatewayMock { get; } = new();

    public IntegrationTestWebAppFactory()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            // Remove the existing AppDbContext registration added by UseAppData()
            var dbContextDescriptor = services.SingleOrDefault(d =>
                d.ServiceType == typeof(DbContextOptions<AppDbContext>)
            );
            if (dbContextDescriptor != null)
            {
                services.Remove(dbContextDescriptor);
            }

            // Register AppDbContext with the shared in-memory SQLite connection
            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseSqlite(_connection);
            });

            // Replace the IPokemonGateway with a mock to avoid external HTTP calls
            var gatewayDescriptor = services.SingleOrDefault(d =>
                d.ServiceType == typeof(IPokemonGateway)
            );
            if (gatewayDescriptor != null)
            {
                services.Remove(gatewayDescriptor);
            }

            services.AddScoped(_ => PokemonGatewayMock.Object);

            // Configure Mock Authentication
            services.AddAuthentication(TestAuthHandler.AuthenticationScheme)
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(TestAuthHandler.AuthenticationScheme, options => { });
            
            // Override the default authorization policy to require our Test scheme
            services.AddAuthorization(options =>
            {
                options.DefaultPolicy = new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder()
                    .AddAuthenticationSchemes(TestAuthHandler.AuthenticationScheme)
                    .RequireAuthenticatedUser()
                    .Build();
            });
        });

        // Add the authorization header to the default client
        builder.ConfigureTestServices(services =>
        {
            services.AddMvc(options =>
            {
                options.Filters.Add(new Microsoft.AspNetCore.Mvc.Authorization.AuthorizeFilter(
                    new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder()
                        .AddAuthenticationSchemes(TestAuthHandler.AuthenticationScheme)
                        .RequireAuthenticatedUser()
                        .Build()));
            });
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
        {
            _connection.Dispose();
        }
    }
}
