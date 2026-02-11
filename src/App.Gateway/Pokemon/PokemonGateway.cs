using App.Core.Pokemon;
using App.Gateway.Client;

namespace App.Gateway.Pokemon;

/// <summary>
/// Gateway implementation for interacting with the PokeAPI.
/// </summary>
/// <param name="pokeClient">The HTTP client for PokeAPI requests.</param>
public class PokemonGateway(IPokeClient pokeClient) : IPokemonGateway
{
    private readonly IPokeClient _pokeClient =
        pokeClient ?? throw new ArgumentNullException(nameof(pokeClient));

    /// <summary>
    /// Fetches a list of Pokemon from the PokeAPI.
    /// </summary>
    /// <param name="limit">The maximum number of Pokemon to return.</param>
    /// <param name="offset">The offset for pagination.</param>
    /// <returns>A list of Pokemon, or null if the request fails.</returns>
    public async Task<IEnumerable<Core.Pokemon.Pokemon>?> FetchPokemonListAsync(
        int limit = 20,
        int offset = 0
    )
    {
        var url = $"pokemon?limit={limit}&offset={offset}";
        var response = await _pokeClient
            .GetAsync<PokeResponse<PokemonItem>>(url)
            .ConfigureAwait(false);

        if (response == null || response.Results == null)
        {
            return null;
        }

        return response
            .Results.Select(item => new Core.Pokemon.Pokemon { Name = item.Name, Url = item.Url })
            .ToList();
    }

    /// <summary>
    /// Fetches details of a specific Pokemon by its ID.
    /// </summary>
    /// <param name="id">The ID of the Pokemon.</param>
    /// <returns>The Pokemon details, or null if not found.</returns>
    public async Task<Core.Pokemon.Pokemon?> FetchPokemonByIdAsync(int id)
    {
        var url = $"pokemon/{id}";
        var response = await _pokeClient.GetAsync<PokemonDetail>(url).ConfigureAwait(false);

        if (response == null)
        {
            return null;
        }

        return new Core.Pokemon.Pokemon
        {
            Name = response.Name,
            Url = $"https://pokeapi.co/api/v2/pokemon/{id}/",
        };
    }

    /// <summary>
    /// Internal class for deserializing Pokemon list items from PokeAPI.
    /// </summary>
    private class PokemonItem
    {
        public string Name { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
    }

    /// <summary>
    /// Internal class for deserializing Pokemon details from PokeAPI.
    /// </summary>
    private class PokemonDetail
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Height { get; set; }
        public int Weight { get; set; }
    }
}
