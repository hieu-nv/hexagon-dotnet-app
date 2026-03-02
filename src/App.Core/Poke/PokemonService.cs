using Microsoft.Extensions.Logging;

namespace App.Core.Poke;

/// <summary>
/// Service layer for Pokemon operations, encapsulating business logic.
/// </summary>
public sealed partial class PokemonService(IPokemonGateway pokemonGateway, ILogger<PokemonService> logger)
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
        LogGettingPokemonList(limit, offset);

        var clampedLimit = Math.Clamp(limit, 1, 100);
        var clampedOffset = Math.Max(0, offset);

        if (clampedLimit != limit)
        {
            LogLimitAdjusted(limit, clampedLimit);
        }

        if (clampedOffset != offset)
        {
            LogOffsetAdjusted(offset, clampedOffset);
        }

        return await _pokemonGateway
            .FetchPokemonListAsync(clampedLimit, clampedOffset)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Fetches details of a specific Pokemon by its ID with business validation.
    /// </summary>
    /// <param name="id">The ID of the Pokemon.</param>
    /// <returns>The Pokemon details, or null if not found.</returns>
    public async Task<Pokemon?> GetPokemonByIdAsync(int id)
    {
        LogGettingPokemonById(id);

        if (id <= 0)
        {
            LogInvalidPokemonId(id);
            throw new ArgumentException("Pokemon ID must be greater than zero.", nameof(id));
        }

        var pokemon = await _pokemonGateway.FetchPokemonByIdAsync(id).ConfigureAwait(false);

        // We could add business logic here like modifying the response or
        // caching it if this service layer supported caching.

        return pokemon;
    }

    [LoggerMessage(LogLevel.Information, "Service getting Pokemon list: Limit={Limit}, Offset={Offset}")]
    private partial void LogGettingPokemonList(int limit, int offset);

    [LoggerMessage(LogLevel.Warning, "Limit adjusted from {OriginalLimit} to {ClampedLimit}.")]
    private partial void LogLimitAdjusted(int originalLimit, int clampedLimit);

    [LoggerMessage(LogLevel.Warning, "Offset adjusted from {OriginalOffset} to {ClampedOffset}.")]
    private partial void LogOffsetAdjusted(int originalOffset, int clampedOffset);

    [LoggerMessage(LogLevel.Information, "Service getting Pokemon by ID: {PokemonId}")]
    private partial void LogGettingPokemonById(int pokemonId);

    [LoggerMessage(LogLevel.Warning, "Invalid Pokemon ID requested: {PokemonId}")]
    private partial void LogInvalidPokemonId(int pokemonId);
}
