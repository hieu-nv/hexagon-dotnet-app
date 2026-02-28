using App.Core.Pokemon;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace App.Api.Tests.Integration;

/// <summary>
/// Custom WebApplicationFactory for integration tests.
/// Replaces external dependencies (Pokemon gateway) with mocks
/// and uses an in-memory SQLite database.
/// </summary>
public class IntegrationTestWebAppFactory : WebApplicationFactory<Program>
{
    public Mock<IPokemonGateway> PokemonGatewayMock { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            // Replace the IPokemonGateway with a mock to avoid external HTTP calls
            var gatewayDescriptor = services.SingleOrDefault(d =>
                d.ServiceType == typeof(IPokemonGateway)
            );
            if (gatewayDescriptor != null)
            {
                services.Remove(gatewayDescriptor);
            }

            services.AddScoped(_ => PokemonGatewayMock.Object);
        });
    }
}
