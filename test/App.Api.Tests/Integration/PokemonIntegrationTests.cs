using System.Net;
using System.Net.Http.Json;
using App.Api.Poke;
using Moq;
using Xunit;

namespace App.Api.Tests.Integration;

public class PokemonIntegrationTests : IClassFixture<IntegrationTestWebAppFactory>
{
    private readonly HttpClient _client;
    private readonly IntegrationTestWebAppFactory _factory;

    public PokemonIntegrationTests(IntegrationTestWebAppFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetPokemonList_WithMockedGateway_ShouldReturnOk()
    {
        // Arrange
        var pokemonList = new List<App.Core.Poke.Pokemon>
        {
            new() { Name = "bulbasaur", Url = new Uri("https://pokeapi.co/api/v2/pokemon/1/", UriKind.RelativeOrAbsolute) },
            new() { Name = "charmander", Url = new Uri("https://pokeapi.co/api/v2/pokemon/4/", UriKind.RelativeOrAbsolute) },
        };

        _factory
            .PokemonGatewayMock.Setup(x =>
                x.FetchPokemonListAsync(It.IsAny<int>(), It.IsAny<int>())
            )
            .ReturnsAsync(pokemonList);

        // Act
        var response = await _client.GetAsync("/api/v1/pokemon?limit=2&offset=0");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetPokemonById_WithMockedGateway_ShouldReturnOk()
    {
        // Arrange
        var pokemon = new App.Core.Poke.Pokemon
        {
            Name = "pikachu",
            Url = new Uri("https://pokeapi.co/api/v2/pokemon/25/", UriKind.RelativeOrAbsolute),
        };

        _factory.PokemonGatewayMock.Setup(x => x.FetchPokemonByIdAsync(25)).ReturnsAsync(pokemon);

        // Act
        var response = await _client.GetAsync("/api/v1/pokemon/25");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetPokemonById_WhenNotFound_ShouldReturnNotFound()
    {
        // Arrange
        _factory
            .PokemonGatewayMock.Setup(x => x.FetchPokemonByIdAsync(99999))
            .ReturnsAsync((App.Core.Poke.Pokemon?)null);

        // Act
        var response = await _client.GetAsync("/api/v1/pokemon/99999");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
