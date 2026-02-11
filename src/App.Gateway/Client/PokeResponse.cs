namespace App.Gateway.Client;

/// <summary>
/// Generic wrapper class for PokeAPI responses with pagination.
/// </summary>
/// <typeparam name="T">The type of results in the response.</typeparam>
public class PokeResponse<T>
{
    /// <summary>
    /// Gets or sets the total count of items.
    /// </summary>
    public int Count { get; set; }

    /// <summary>
    /// Gets or sets the URL for the next page of results.
    /// </summary>
    public string? Next { get; set; }

    /// <summary>
    /// Gets or sets the URL for the previous page of results.
    /// </summary>
    public string? Previous { get; set; }

    /// <summary>
    /// Gets or sets the list of results.
    /// </summary>
    public List<T> Results { get; set; } = [];
}
