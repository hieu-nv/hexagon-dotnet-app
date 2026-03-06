using System.Text.Json;
using System.Threading.RateLimiting;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.CircuitBreaker;
using Polly.Fallback;
using Polly.RateLimiting;
using Polly.Retry;
using Polly.Timeout;

namespace App.Gateway.Client;

/// <summary>
/// HTTP client for interacting with the PokeAPI.
/// </summary>
/// <param name="httpClient">The HTTP client instance.</param>
public partial class PokeClient(HttpClient httpClient) : IPokeClient
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    private static readonly ResiliencePipeline<HttpResponseMessage> ResiliencePipeline =
        new ResiliencePipelineBuilder<HttpResponseMessage>()
            .AddFallback(
                new FallbackStrategyOptions<HttpResponseMessage>
                {
                    FallbackAction = args =>
                    {
                        // Fallback to a mock 503 response when all other strategies fail
                        var response = new HttpResponseMessage(
                            System.Net.HttpStatusCode.ServiceUnavailable
                        )
                        {
                            Content = new StringContent("{}"), // Safe empty JSON
                        };
                        return Outcome.FromResultAsValueTask(response);
                    },
                    ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
                        .Handle<HttpRequestException>()
                        .Handle<BrokenCircuitException>()
                        .Handle<TimeoutRejectedException>()
                        .Handle<RateLimiterRejectedException>()
                        .HandleResult(response => !response.IsSuccessStatusCode),
                }
            )
            .AddRateLimiter(
                new SlidingWindowRateLimiter(
                    new SlidingWindowRateLimiterOptions
                    {
                        PermitLimit = 100,
                        Window = TimeSpan.FromSeconds(60),
                        SegmentsPerWindow = 6,
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = 50,
                    }
                )
            )
            .AddTimeout(new TimeoutStrategyOptions { Timeout = TimeSpan.FromSeconds(15) })
            .AddRetry(
                new RetryStrategyOptions<HttpResponseMessage>
                {
                    ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
                        .Handle<HttpRequestException>()
                        .HandleResult(response =>
                            !response.IsSuccessStatusCode
                            && response.StatusCode != System.Net.HttpStatusCode.ServiceUnavailable
                        ),
                    MaxRetryAttempts = 3,
                    Delay = TimeSpan.FromSeconds(2),
                    BackoffType = DelayBackoffType.Exponential,
                    UseJitter = true,
                }
            )
            .AddCircuitBreaker(
                new CircuitBreakerStrategyOptions<HttpResponseMessage>
                {
                    ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
                        .Handle<HttpRequestException>()
                        .HandleResult(response =>
                            !response.IsSuccessStatusCode
                            && response.StatusCode != System.Net.HttpStatusCode.ServiceUnavailable
                        ),
                    FailureRatio = 0.5,
                    SamplingDuration = TimeSpan.FromSeconds(10),
                    MinimumThroughput = 5,
                    BreakDuration = TimeSpan.FromSeconds(30),
                }
            )
            .Build();

    private readonly HttpClient _httpClient =
        httpClient ?? throw new ArgumentNullException(nameof(httpClient));

    /// <summary>
    /// Performs an HTTP GET request to the specified URL.
    /// </summary>
    /// <typeparam name="T">The type of the response.</typeparam>
    /// <param name="url">The URL to request.</param>
    /// <returns>The response deserialized to the specified type, or null if the request fails.</returns>
    public async Task<T?> GetAsync<T>(Uri url)
        where T : class
    {
        var response = await ResiliencePipeline
            .ExecuteAsync(async cancellationToken =>
            {
                var req = await _httpClient
                    .GetAsync(url, cancellationToken)
                    .ConfigureAwait(false);
                req.EnsureSuccessStatusCode();
                return req;
            })
            .ConfigureAwait(false);

        var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        return JsonSerializer.Deserialize<T>(content, JsonOptions);
    }
}
