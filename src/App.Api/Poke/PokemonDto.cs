namespace App.Api.Poke;

/// <summary>
/// Represents a Pokemon returned by the API.
/// </summary>
internal record PokemonResponse(string Name, Uri Url);

/// <summary>
/// Represents a list of Pokemon returned by the API.
/// </summary>
internal record PokemonListResponse(
    int Count,
    Uri? Next,
    Uri? Previous,
    IEnumerable<PokemonResponse> Results
);
