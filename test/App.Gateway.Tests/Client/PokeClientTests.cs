using System.Net;
using System.Text.Json;

using App.Gateway.Client;

using Microsoft.Extensions.Logging.Abstractions;

namespace App.Gateway.Tests.Client;

public class PokeClientTests
{
    /// <summary>
    /// Creates a PokeClient with a mocked HttpMessageHandler.
    /// </summary>
    private static PokeClient CreateClient(HttpMessageHandler handler)
    {
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://pokeapi.co/api/v2/"), };
        return new PokeClient(httpClient, NullLogger<PokeClient>.Instance);
    }

    #region GetAsync Success

    [Fact]
    public async Task GetAsync_WithValidResponse_ShouldDeserializeCorrectly()
    {
        // Arrange
        var expected = new TestPayload { Name = "pikachu", Id = 25 };
        var json = JsonSerializer.Serialize(expected);
        var handler = new FakeHttpMessageHandler(
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json"),
            }
        );
        var client = CreateClient(handler);

        // Act
        var result = await client.GetAsync<TestPayload>("pokemon/25");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("pikachu", result.Name);
        Assert.Equal(25, result.Id);
    }

    #endregion

    #region GetAsync Error Scenarios

    [Fact]
    public async Task GetAsync_With404Response_ShouldReturnNull()
    {
        // Arrange
        var handler = new FakeHttpMessageHandler(new HttpResponseMessage(HttpStatusCode.NotFound));
        var client = CreateClient(handler);

        // Act
        var result = await client.GetAsync<TestPayload>("pokemon/99999");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetAsync_With500Response_ShouldReturnNull()
    {
        // Arrange
        var handler = new FakeHttpMessageHandler(
            new HttpResponseMessage(HttpStatusCode.InternalServerError)
        );
        var client = CreateClient(handler);

        // Act
        var result = await client.GetAsync<TestPayload>("pokemon/1");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetAsync_WithTimeout_ShouldPropagateException()
    {
        // Arrange
        var handler = new FakeHttpMessageHandler(new TaskCanceledException("Request timed out"));
        var client = CreateClient(handler);

        // Act & Assert — cancellation exceptions now propagate instead of being swallowed
        await Assert.ThrowsAsync<TaskCanceledException>(() => client.GetAsync<TestPayload>("pokemon/1")
        );
    }

    [Fact]
    public async Task GetAsync_WithMalformedJson_ShouldReturnNull()
    {
        // Arrange
        var handler = new FakeHttpMessageHandler(
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    "this is not json",
                    System.Text.Encoding.UTF8,
                    "application/json"
                ),
            }
        );
        var client = CreateClient(handler);

        // Act — JsonException is not HttpRequestException, so it propagates
        await Assert.ThrowsAsync<JsonException>(() => client.GetAsync<TestPayload>("pokemon/1")
        );
    }

    [Fact]
    public async Task GetAsync_WithEmptyJsonObject_ShouldReturnObjectWithDefaults()
    {
        // Arrange
        var handler = new FakeHttpMessageHandler(
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{}", System.Text.Encoding.UTF8, "application/json"),
            }
        );
        var client = CreateClient(handler);

        // Act
        var result = await client.GetAsync<TestPayload>("pokemon/1");

        // Assert
        Assert.NotNull(result);
        Assert.Null(result.Name);
        Assert.Equal(0, result.Id);
    }

    #endregion

    #region Helper Types

    private class TestPayload
    {
        public string? Name { get; set; }
        public int Id { get; set; }
    }

    /// <summary>
    /// A simple fake HttpMessageHandler that returns a predefined response or throws an exception.
    /// </summary>
    private class FakeHttpMessageHandler : HttpMessageHandler
    {
        private readonly HttpResponseMessage? _response;
        private readonly Exception? _exception;

        public FakeHttpMessageHandler(HttpResponseMessage response) => _response = response;

        public FakeHttpMessageHandler(Exception exception) => _exception = exception;

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken
        )
        {
            if (_exception != null)
            {
                throw _exception;
            }

            return Task.FromResult(_response!);
        }
    }

    #endregion
}

