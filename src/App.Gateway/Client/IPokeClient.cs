namespace App.Gateway.Client;

/// <summary>
/// Interface for the PokeAPI client.
/// </summary>
public interface IPokeClient
{
    /// <summary>
    /// Performs an HTTP GET request to the specified URL.
    /// </summary>
    /// <typeparam name="T">The type of the response.</typeparam>
    /// <param name="url">The URL to request.</param>
    /// <returns>The response deserialized to the specified type, or null if the request fails.</returns>
    Task<T?> GetAsync<T>(string url)
        where T : class;
}
