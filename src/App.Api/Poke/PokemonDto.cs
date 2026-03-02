namespace App.Api.Poke;

/// <summary>
/// Represents a Pokemon returned by the API.
/// </summary>
public record PokemonResponse(string Name, string Url);

/// <summary>
/// Represents a list of Pokemon returned by the API.
/// </summary>
public record PokemonListResponse(
    int Count,
    string? Next,
    string? Previous,
    IEnumerable<PokemonResponse> Results
);
