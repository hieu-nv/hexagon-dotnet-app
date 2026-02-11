using App.Api.Pokemon;
using App.Core.Pokemon;

namespace Microsoft.AspNetCore.Builder;

/// <summary>
/// Extension methods for configuring the PokemonEndpoints in the application.
/// </summary>
internal static class PokemonEndpointsExtensions
{
    /// <summary>
    /// Adds the PokemonEndpoints to the application builder.
    /// </summary>
    /// <param name="app">The web application builder.</param>
    /// <returns>The web application builder.</returns>
    public static WebApplicationBuilder UsePokemon(this WebApplicationBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);
        app.Services.AddScoped<PokemonEndpoints>();
        return app;
    }

    /// <summary>
    /// Maps the PokemonEndpoints to the specified route builder.
    /// </summary>
    /// <param name="endpointRouteBuilder">The endpoint route builder.</param>
    /// <returns>The endpoint route builder.</returns>
    public static IEndpointRouteBuilder UsePokemon(this IEndpointRouteBuilder endpointRouteBuilder)
    {
        ArgumentNullException.ThrowIfNull(endpointRouteBuilder);

        var group = endpointRouteBuilder.MapGroup("/api/v1/pokemon").WithTags("Pokemon");

        // GET endpoints
        group
            .MapGet(
                "/",
                (PokemonEndpoints handler, int limit = 20, int offset = 0) =>
                    handler.FetchPokemonListAsync(limit, offset)
            )
            .WithName("GetPokemonList")
            .WithSummary("Get a list of Pokemon from PokeAPI")
            .WithDescription("Fetches Pokemon from the external PokeAPI with pagination support")
            .Produces<IEnumerable<Pokemon>>(200)
            .Produces(502);

        group
            .MapGet(
                "/{id:int}",
                (PokemonEndpoints handler, int id) => handler.FetchPokemonByIdAsync(id)
            )
            .WithName("GetPokemonById")
            .WithSummary("Get a specific Pokemon by ID")
            .WithDescription("Fetches details of a specific Pokemon from the external PokeAPI")
            .Produces<Pokemon>(200)
            .Produces(404);

        return endpointRouteBuilder;
    }
}
