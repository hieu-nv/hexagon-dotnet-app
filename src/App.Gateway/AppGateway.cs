using App.Core.Pokemon;
using App.Gateway.Client;
using App.Gateway.Pokemon;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNetCore.Builder;

/// <summary>
/// Provides methods to configure the application gateway layer for external service integrations.
/// </summary>
public static class AppGateway
{
    /// <summary>
    /// Configures the application to use the gateway layer with external service clients.
    /// </summary>
    /// <param name="builder">The web application builder.</param>
    /// <returns>The web application builder.</returns>
    public static WebApplicationBuilder UseAppGateway(this WebApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        // Register HTTP client for PokeAPI
        builder
            .Services.AddHttpClient<IPokeClient, PokeClient>(client =>
            {
                client.BaseAddress = new Uri("https://pokeapi.co/api/v2/");
                client.DefaultRequestHeaders.Add("Accept", "application/json");
                client.Timeout = TimeSpan.FromSeconds(30);
            })
            .AddStandardResilienceHandler(options =>
            {
                // Don't retry on 404 - these are expected for invalid Pokemon IDs
                options.Retry.ShouldHandle = args =>
                    ValueTask.FromResult(
                        args.Outcome.Result?.StatusCode != System.Net.HttpStatusCode.NotFound
                    );
            });

        // Register gateway implementations
        builder.Services.AddScoped<IPokemonGateway, PokemonGateway>();

        return builder;
    }
}
