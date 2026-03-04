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

namespace App.Api.Tests.Integration;

/// <summary>
/// Auth handler that provides an unauthenticated (anonymous) user.
/// </summary>
internal class AnonymousAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string AuthenticationScheme = "Anonymous";

    public AnonymousAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder) { }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        => Task.FromResult(AuthenticateResult.NoResult());
}

/// <summary>
/// Factory configured with an anonymous (unauthenticated) user — used to test 401 responses.
/// </summary>
#pragma warning disable CA1515
public class AnonymousWebAppFactory : WebApplicationFactory<Program>
{
    private readonly SqliteConnection _connection;
    public Mock<IPokemonGateway> PokemonGatewayMock { get; } = new();

    public AnonymousWebAppFactory()
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

            // Anonymous scheme — no user authenticated
            services.AddAuthentication(AnonymousAuthHandler.AuthenticationScheme)
                .AddScheme<AuthenticationSchemeOptions, AnonymousAuthHandler>(
                    AnonymousAuthHandler.AuthenticationScheme, _ => { });
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
/// Integration tests ensuring protected endpoints return 401 for unauthenticated users
/// and 200/403 for authenticated users with various roles.
/// </summary>
#pragma warning disable CA1515
public class AuthorizedEndpointsTests : IClassFixture<AnonymousWebAppFactory>, IClassFixture<IntegrationTestWebAppFactory>, IClassFixture<NonAdminWebAppFactory>
{
    private readonly HttpClient _anonymous;
    private readonly HttpClient _admin;
    private readonly HttpClient _user;

    public AuthorizedEndpointsTests(
        AnonymousWebAppFactory anonymousFactory,
        IntegrationTestWebAppFactory adminFactory,
        NonAdminWebAppFactory userFactory)
    {
        _anonymous = anonymousFactory.CreateClient();
        _admin = adminFactory.CreateClient();
        _user = userFactory.CreateClient();
    }

    // ─── /auth/status — open endpoint ─────────────────────────────────────

    [Fact]
    public async Task AuthStatus_Unauthenticated_Returns401()
    {
        var response = await _anonymous.GetAsync("/auth/status");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task AuthStatus_Authenticated_Returns200()
    {
        var response = await _admin.GetAsync("/auth/status");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    // ─── /auth/me — requires authentication ───────────────────────────────

    [Fact]
    public async Task AuthMe_Unauthenticated_Returns401()
    {
        var response = await _anonymous.GetAsync("/auth/me");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task AuthMe_Authenticated_Returns200()
    {
        var response = await _admin.GetAsync("/auth/me");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    // ─── /admin/stats — requires AdminOnly policy ─────────────────────────

    [Fact]
    public async Task AdminStats_Unauthenticated_Returns401()
    {
        var response = await _anonymous.GetAsync("/admin/stats");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task AdminStats_AdminUser_Returns200()
    {
        var response = await _admin.GetAsync("/admin/stats");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task AdminStats_NonAdminUser_ReturnsForbidden()
    {
        var response = await _user.GetAsync("/admin/stats");
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    // ─── Todo protected endpoints ─────────────────────────────────────────

    [Fact]
    public async Task TodoCreate_Unauthenticated_Returns401()
    {
        var request = new App.Api.Todo.CreateTodoRequest("Test Todo", false, null);
        var response = await _anonymous.PostAsJsonAsync("/api/v1/todos", request);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task TodoCreate_WithUserRole_Returns201()
    {
        var request = new App.Api.Todo.CreateTodoRequest("Test User Todo", false, null);
        var response = await _user.PostAsJsonAsync("/api/v1/todos", request);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task TodoDelete_Unauthenticated_Returns401()
    {
        // Any non-existent ID — should fail at auth before reaching the handler
        var response = await _anonymous.DeleteAsync("/api/v1/todos/999");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task TodoGet_Unauthenticated_Returns200()
    {
        // GET /todos is publicly accessible
        var response = await _anonymous.GetAsync("/api/v1/todos");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
#pragma warning restore CA1515
