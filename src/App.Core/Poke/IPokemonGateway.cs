namespace App.Core.Poke;

/// <summary>
/// Gateway interface for interacting with external Pokemon services.
/// </summary>
public interface IPokemonGateway
{
    /// <summary>
    /// Fetches a list of Pokemon from the external API.
    /// </summary>
    /// <param name="limit">The maximum number of Pokemon to return.</param>
    /// <param name="offset">The offset for pagination.</param>
    /// <returns>A list of Pokemon, or null if the request fails.</returns>
    Task<IEnumerable<Pokemon>?> FetchPokemonListAsync(int limit = 20, int offset = 0);

    /// <summary>
    /// Fetches details of a specific Pokemon by its ID.
    /// </summary>
    /// <param name="id">The ID of the Pokemon.</param>
    /// <returns>The Pokemon details, or null if not found.</returns>
    Task<Pokemon?> FetchPokemonByIdAsync(int id);
}
