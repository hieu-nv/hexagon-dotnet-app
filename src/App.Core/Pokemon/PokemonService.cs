using Microsoft.Extensions.Logging;

namespace App.Core.Pokemon;

/// <summary>
/// Service layer for Pokemon operations, encapsulating business logic.
/// </summary>
public sealed class PokemonService(IPokemonGateway pokemonGateway, ILogger<PokemonService> logger)
{
    private readonly IPokemonGateway _pokemonGateway =
        pokemonGateway ?? throw new ArgumentNullException(nameof(pokemonGateway));
    private readonly ILogger<PokemonService> _logger =
        logger ?? throw new ArgumentNullException(nameof(logger));

    /// <summary>
    /// Fetches a list of Pokemon from the external API with business validation.
    /// </summary>
    /// <param name="limit">The maximum number of Pokemon to return.</param>
    /// <param name="offset">The offset for pagination.</param>
    /// <returns>A list of Pokemon, or null if the request fails.</returns>
    public async Task<IEnumerable<Pokemon>?> GetPokemonListAsync(int limit = 20, int offset = 0)
    {
        _logger.LogInformation(
            "Service getting Pokemon list: Limit={Limit}, Offset={Offset}",
            limit,
            offset
        );

        if (limit <= 0)
        {
            _logger.LogWarning("Invalid limit requested: {Limit}. Setting to default 20.", limit);
            limit = 20;
        }

        if (limit > 100)
        {
            _logger.LogWarning("Limit too high requested: {Limit}. Setting to max 100.", limit);
            limit = 100;
        }

        if (offset < 0)
        {
            _logger.LogWarning("Invalid offset requested: {Offset}. Setting to 0.", offset);
            offset = 0;
        }

        return await _pokemonGateway.FetchPokemonListAsync(limit, offset).ConfigureAwait(false);
    }

    /// <summary>
    /// Fetches details of a specific Pokemon by its ID with business validation.
    /// </summary>
    /// <param name="id">The ID of the Pokemon.</param>
    /// <returns>The Pokemon details, or null if not found.</returns>
    public async Task<Pokemon?> GetPokemonByIdAsync(int id)
    {
        _logger.LogInformation("Service getting Pokemon by ID: {PokemonId}", id);

        if (id <= 0)
        {
            _logger.LogWarning("Invalid Pokemon ID requested: {PokemonId}", id);
            throw new ArgumentException("Pokemon ID must be greater than zero.", nameof(id));
        }

        var pokemon = await _pokemonGateway.FetchPokemonByIdAsync(id).ConfigureAwait(false);

        // We could add business logic here like modifying the response or
        // caching it if this service layer supported caching.

        return pokemon;
    }
}
