using App.Core.Poke;

namespace App.Api.Poke;

/// <summary>
/// Extension methods for mapping Pokemon-related entities and DTOs.
/// </summary>
public static class PokemonMappingExtensions
{
    /// <summary>
    /// Maps a Pokemon to a PokemonResponse.
    /// </summary>
    /// <param name="pokemon">The Pokemon to map.</param>
    /// <returns>The mapped response.</returns>
    public static PokemonResponse ToResponse(this App.Core.Poke.Pokemon pokemon)
    {
        return new PokemonResponse(pokemon.Name, pokemon.Url?.ToString() ?? string.Empty);
    }
}
