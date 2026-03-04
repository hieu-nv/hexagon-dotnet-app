using System.Net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using App.Core.Poke;
using App.Data;
using Moq;

namespace App.Api.Tests.Integration;

/// <summary>
/// Factory with an authenticated user for logout endpoint testing.
/// Uses the existing <see cref="TestAuthHandler"/> to simulate an admin+user authenticated session.
/// </summary>
#pragma warning disable CA1515
public class LogoutTestWebAppFactory : WebApplicationFactory<Program>
{
    private readonly SqliteConnection _connection;
    public Mock<IPokemonGateway> PokemonGatewayMock { get; } = new();

    public LogoutTestWebAppFactory()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            var dbCtx = services.SingleOrDefault(d =>
                d.ServiceType == typeof(DbContextOptions<AppDbContext>));
            if (dbCtx != null) services.Remove(dbCtx);

            services.AddDbContext<AppDbContext>(o => o.UseSqlite(_connection));

            var gw = services.SingleOrDefault(d => d.ServiceType == typeof(IPokemonGateway));
            if (gw != null) services.Remove(gw);
            services.AddScoped(_ => PokemonGatewayMock.Object);

            // Reuse admin TestAuthHandler
            services.AddAuthentication(TestAuthHandler.AuthenticationScheme)
                .AddScheme<Microsoft.AspNetCore.Authentication.AuthenticationSchemeOptions, TestAuthHandler>(
                    TestAuthHandler.AuthenticationScheme, _ => { });

            services.AddAuthorization(options =>
            {
                options.DefaultPolicy = new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder()
                    .AddAuthenticationSchemes(TestAuthHandler.AuthenticationScheme)
                    .RequireAuthenticatedUser()
                    .Build();
            });
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing) _connection.Dispose();
    }
}
#pragma warning restore CA1515

/// <summary>
/// Integration tests for the <c>POST /auth/logout</c> endpoint.
/// </summary>
#pragma warning disable CA1515
public class LogoutTests : IClassFixture<LogoutTestWebAppFactory>, IClassFixture<AnonymousWebAppFactory>
{
    private readonly HttpClient _authenticated;
    private readonly HttpClient _anonymous;

    public LogoutTests(LogoutTestWebAppFactory authFactory, AnonymousWebAppFactory anonFactory)
    {
        _authenticated = authFactory.CreateClient();
        _anonymous = anonFactory.CreateClient();
    }

    [Fact]
    public async Task Logout_Unauthenticated_Returns401()
    {
        var response = await _anonymous.PostAsync("/auth/logout", null);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Logout_AuthenticatedUser_Returns200()
    {
        var response = await _authenticated.PostAsync("/auth/logout", null);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Logout_AuthenticatedUser_ResponseContainsLoggedOutTrue()
    {
        var response = await _authenticated.PostAsync("/auth/logout", null);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("loggedOut", content, StringComparison.OrdinalIgnoreCase);
    }
}
#pragma warning restore CA1515
