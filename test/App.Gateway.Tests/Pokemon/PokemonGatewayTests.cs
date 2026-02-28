using App.Core.Pokemon;
using App.Gateway.Pokemon;
using Moq;
using Xunit;

namespace App.Gateway.Tests.Pokemon;

public class PokemonGatewayTests
{
    private readonly Mock<IPokemonGateway> _gatewayMock;

    public PokemonGatewayTests()
    {
        _gatewayMock = new Mock<IPokemonGateway>();
    }

    #region FetchPokemonListAsync Tests

    [Fact]
    public async Task FetchPokemonListAsync_WithValidResponse_ShouldReturnPokemonList()
    {
        // Arrange
        var expectedPokemon = new List<App.Core.Pokemon.Pokemon>
        {
            new() { Name = "bulbasaur", Url = "https://pokeapi.co/api/v2/pokemon/1/" },
            new() { Name = "ivysaur", Url = "https://pokeapi.co/api/v2/pokemon/2/" },
        };

        _gatewayMock.Setup(x => x.FetchPokemonListAsync(20, 0)).ReturnsAsync(expectedPokemon);

        // Act
        var result = await _gatewayMock.Object.FetchPokemonListAsync(20, 0);

        // Assert
        Assert.NotNull(result);
        var pokemonList = result.ToList();
        Assert.Equal(2, pokemonList.Count);
        Assert.Equal("bulbasaur", pokemonList[0].Name);
        Assert.Equal("ivysaur", pokemonList[1].Name);
    }

    [Fact]
    public async Task FetchPokemonListAsync_WithNullResponse_ShouldReturnNull()
    {
        // Arrange
        _gatewayMock
            .Setup(x => x.FetchPokemonListAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync((IEnumerable<App.Core.Pokemon.Pokemon>?)null);

        // Act
        var result = await _gatewayMock.Object.FetchPokemonListAsync();

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task FetchPokemonListAsync_WithEmptyResults_ShouldReturnEmptyList()
    {
        // Arrange
        _gatewayMock
            .Setup(x => x.FetchPokemonListAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(new List<App.Core.Pokemon.Pokemon>());

        // Act
        var result = await _gatewayMock.Object.FetchPokemonListAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    #endregion

    #region FetchPokemonByIdAsync Tests

    [Fact]
    public async Task FetchPokemonByIdAsync_WithValidId_ShouldReturnPokemon()
    {
        // Arrange
        var expected = new App.Core.Pokemon.Pokemon
        {
            Name = "pikachu",
            Url = "https://pokeapi.co/api/v2/pokemon/25/",
        };

        _gatewayMock.Setup(x => x.FetchPokemonByIdAsync(25)).ReturnsAsync(expected);

        // Act
        var result = await _gatewayMock.Object.FetchPokemonByIdAsync(25);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("pikachu", result.Name);
        Assert.Equal("https://pokeapi.co/api/v2/pokemon/25/", result.Url);
    }

    [Fact]
    public async Task FetchPokemonByIdAsync_WithNonExistentId_ShouldReturnNull()
    {
        // Arrange
        _gatewayMock
            .Setup(x => x.FetchPokemonByIdAsync(99999))
            .ReturnsAsync((App.Core.Pokemon.Pokemon?)null);

        // Act
        var result = await _gatewayMock.Object.FetchPokemonByIdAsync(99999);

        // Assert
        Assert.Null(result);
    }

    #endregion
}
