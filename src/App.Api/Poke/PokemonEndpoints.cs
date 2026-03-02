using App.Core.Poke;

namespace App.Api.Poke;

/// <summary>
/// Endpoints for Pokemon operations via external gateway.
/// </summary>
/// <param name="pokemonGateway">The Pokemon gateway for accessing external API.</param>
/// <param name="logger">The logger for tracking endpoint activity.</param>
internal sealed class PokemonEndpoints(
    PokemonService pokemonService,
    ILogger<PokemonEndpoints> logger
)
{
    private readonly PokemonService _pokemonService =
        pokemonService ?? throw new ArgumentNullException(nameof(pokemonService));
    private readonly ILogger<PokemonEndpoints> _logger =
        logger ?? throw new ArgumentNullException(nameof(logger));

    /// <summary>
    /// Retrieves a list of Pokemon from the external API.
    /// </summary>
    /// <param name="limit">The maximum number of Pokemon to return.</param>
    /// <param name="offset">The offset for pagination.</param>
    /// <returns>A list of Pokemon.</returns>
    public async Task<IResult> FetchPokemonListAsync(int limit = 20, int offset = 0)
    {
        _logger.LogInformation(
            "Fetching Pokemon list: Limit={Limit}, Offset={Offset}",
            limit,
            offset
        );

        var pokemon = await _pokemonService
            .GetPokemonListAsync(limit, offset)
            .ConfigureAwait(false);

        if (pokemon == null)
        {
            _logger.LogError(
                "Failed to fetch Pokemon list from external API. Limit={Limit}, Offset={Offset}",
                limit,
                offset
            );
            return Results.Problem(
                title: "Failed to fetch Pokemon",
                statusCode: StatusCodes.Status502BadGateway
            );
        }

        var count = pokemon.Count();
        _logger.LogInformation(
            "Successfully fetched {Count} Pokemon. Limit={Limit}, Offset={Offset}",
            count,
            limit,
            offset
        );

        var results = pokemon.Select(p => p.ToResponse());
        string? next =
            count == limit ? $"/api/v1/pokemon?limit={limit}&offset={offset + limit}" : null;
        string? previous =
            offset > 0
                ? $"/api/v1/pokemon?limit={limit}&offset={Math.Max(0, offset - limit)}"
                : null;

        var response = new PokemonListResponse(count, next, previous, results);

        return Results.Ok(response);
    }

    /// <summary>
    /// Retrieves a specific Pokemon by its ID from the external API.
    /// </summary>
    /// <param name="id">The ID of the Pokemon.</param>
    /// <returns>The Pokemon details.</returns>
    public async Task<IResult> FetchPokemonByIdAsync(int id)
    {
        _logger.LogInformation("Fetching Pokemon by ID: {PokemonId}", id);

        var pokemon = await _pokemonService.GetPokemonByIdAsync(id).ConfigureAwait(false);

        if (pokemon == null)
        {
            _logger.LogWarning("Pokemon not found. PokemonId={PokemonId}", id);
            return Results.NotFound(new { message = $"Pokemon with ID {id} not found" });
        }

        _logger.LogInformation(
            "Successfully fetched Pokemon: {PokemonName} (ID: {PokemonId})",
            pokemon.Name,
            id
        );

        return Results.Ok(pokemon.ToResponse());
    }
}
