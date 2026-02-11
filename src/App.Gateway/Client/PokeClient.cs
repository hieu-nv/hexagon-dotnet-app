using System.Text.Json;

namespace App.Gateway.Client;

/// <summary>
/// HTTP client for interacting with the PokeAPI.
/// </summary>
/// <param name="httpClient">The HTTP client instance.</param>
public class PokeClient(HttpClient httpClient) : IPokeClient
{
    private readonly HttpClient _httpClient =
        httpClient ?? throw new ArgumentNullException(nameof(httpClient));

    /// <summary>
    /// Performs an HTTP GET request to the specified URL.
    /// </summary>
    /// <typeparam name="T">The type of the response.</typeparam>
    /// <param name="url">The URL to request.</param>
    /// <returns>The response deserialized to the specified type, or null if the request fails.</returns>
    public async Task<T?> GetAsync<T>(string url)
        where T : class
    {
        try
        {
            var response = await _httpClient.GetAsync(url).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            return JsonSerializer.Deserialize<T>(
                content,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );
        }
        catch (Exception ex)
        {
            // Log the exception in production
            Console.Error.WriteLine($"Error fetching data from {url}: {ex.Message}");
            return null;
        }
    }
}
