using App.Core.Pokemon;

namespace App.Api.Pokemon;

/// <summary>
/// Endpoints for Pokemon operations via external gateway.
/// </summary>
/// <param name="pokemonGateway">The Pokemon gateway for accessing external API.</param>
internal sealed class PokemonEndpoints(IPokemonGateway pokemonGateway)
{
    private readonly IPokemonGateway _pokemonGateway =
        pokemonGateway ?? throw new ArgumentNullException(nameof(pokemonGateway));

    /// <summary>
    /// Retrieves a list of Pokemon from the external API.
    /// </summary>
    /// <param name="limit">The maximum number of Pokemon to return.</param>
    /// <param name="offset">The offset for pagination.</param>
    /// <returns>A list of Pokemon.</returns>
    public async Task<IResult> FetchPokemonListAsync(int limit = 20, int offset = 0)
    {
        var pokemon = await _pokemonGateway
            .FetchPokemonListAsync(limit, offset)
            .ConfigureAwait(false);

        if (pokemon == null)
        {
            return Results.Problem(
                title: "Failed to fetch Pokemon",
                statusCode: StatusCodes.Status502BadGateway
            );
        }

        return Results.Ok(pokemon);
    }

    /// <summary>
    /// Retrieves a specific Pokemon by its ID from the external API.
    /// </summary>
    /// <param name="id">The ID of the Pokemon.</param>
    /// <returns>The Pokemon details.</returns>
    public async Task<IResult> FetchPokemonByIdAsync(int id)
    {
        var pokemon = await _pokemonGateway.FetchPokemonByIdAsync(id).ConfigureAwait(false);

        if (pokemon == null)
        {
            return Results.NotFound(new { message = $"Pokemon with ID {id} not found" });
        }

        return Results.Ok(pokemon);
    }
}
