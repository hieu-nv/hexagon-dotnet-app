using App.Core.Poke;
using App.Gateway.Client;
using App.Gateway.Poke;

using Moq;

using Xunit;

namespace App.Gateway.Tests.Pokemon;

public class PokemonGatewayTests
{
    private readonly Mock<IPokeClient> _pokeClientMock;
    private readonly PokemonGateway _gateway;

    public PokemonGatewayTests()
    {
        _pokeClientMock = new Mock<IPokeClient>();
        _gateway = new PokemonGateway(_pokeClientMock.Object);
    }

    #region FetchPokemonListAsync Tests

    [Fact]
    public async Task FetchPokemonListAsync_WithValidResponse_ShouldReturnPokemonList()
    {
        // Arrange
        var pokeResponse = new PokeResponse<PokemonGateway.PokemonItem>
        {
            Results = new List<PokemonGateway.PokemonItem>
            {
                new() { Name = "bulbasaur", Url = "https://pokeapi.co/api/v2/pokemon/1/" },
                new() { Name = "ivysaur", Url = "https://pokeapi.co/api/v2/pokemon/2/" }
            }
        };

        _pokeClientMock.Setup(x => x.GetAsync<PokeResponse<PokemonGateway.PokemonItem>>(It.IsAny<string>()))
            .ReturnsAsync(pokeResponse);

        // Act
        var result = await _gateway.FetchPokemonListAsync(2, 0);

        // Assert
        Assert.NotNull(result);
        var pokemonList = result.ToList();
        Assert.Equal(2, pokemonList.Count);
        Assert.Equal("bulbasaur", pokemonList[0].Name);
        Assert.Equal("https://pokeapi.co/api/v2/pokemon/1/", pokemonList[0].Url);
    }

    [Fact]
    public async Task FetchPokemonListAsync_WithNullResponse_ShouldReturnNull()
    {
        // Arrange
        _pokeClientMock.Setup(x => x.GetAsync<PokeResponse<PokemonGateway.PokemonItem>>(It.IsAny<string>()))
            .ReturnsAsync((PokeResponse<PokemonGateway.PokemonItem>?)null);

        // Act
        var result = await _gateway.FetchPokemonListAsync();

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task FetchPokemonListAsync_WithEmptyResults_ShouldReturnNull()
    {
        // Arrange
        _pokeClientMock.Setup(x => x.GetAsync<PokeResponse<PokemonGateway.PokemonItem>>(It.IsAny<string>()))
            .ReturnsAsync(new PokeResponse<PokemonGateway.PokemonItem> { Results = null! });

        // Act
        var result = await _gateway.FetchPokemonListAsync();

        // Assert
        Assert.Null(result);
    }

    #endregion

    #region FetchPokemonByIdAsync Tests

    [Fact]
    public async Task FetchPokemonByIdAsync_WithValidId_ShouldReturnPokemon()
    {
        // Arrange
        var pokemonDetail = new PokemonGateway.PokemonDetail { Id = 25, Name = "pikachu" };

        _pokeClientMock.Setup(x => x.GetAsync<PokemonGateway.PokemonDetail>(It.IsAny<string>()))
            .ReturnsAsync(pokemonDetail);

        // Act
        var result = await _gateway.FetchPokemonByIdAsync(25);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("pikachu", result.Name);
        Assert.Equal("pokemon/25/", result.Url);
    }

    [Fact]
    public async Task FetchPokemonByIdAsync_WithNonExistentId_ShouldReturnNull()
    {
        // Arrange
        _pokeClientMock.Setup(x => x.GetAsync<PokemonGateway.PokemonDetail>(It.IsAny<string>()))
            .ReturnsAsync((PokemonGateway.PokemonDetail?)null);

        // Act
        var result = await _gateway.FetchPokemonByIdAsync(999);

        // Assert
        Assert.Null(result);
    }

    #endregion

    #region Constructor Tests

    [Fact]
    public void Constructor_WithNullClient_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new PokemonGateway(null!));
    }

    #endregion

    #region DTO Tests

    [Fact]
    public void PokeResponse_Properties_CanBeSet()
    {
        // Arrange
        var response = new PokeResponse<string>
        {
            Count = 100, Next = "next-url", Previous = "prev-url", Results = ["test"]
        };

        // Assert
        Assert.Equal(100, response.Count);
        Assert.Equal("next-url", response.Next);
        Assert.Equal("prev-url", response.Previous);
        Assert.Single(response.Results);
    }

    [Fact]
    public void PokemonDetail_Properties_CanBeSet()
    {
        // Arrange - PokemonGateway internal class
        var detail = new PokemonGateway.PokemonDetail { Id = 25, Name = "pikachu", Height = 4, Weight = 60 };

        // Assert
        Assert.Equal(25, detail.Id);
        Assert.Equal("pikachu", detail.Name);
        Assert.Equal(4, detail.Height);
        Assert.Equal(60, detail.Weight);
    }

    [Fact]
    public void PokemonItem_Properties_CanBeSet()
    {
        // Arrange
        var item = new PokemonGateway.PokemonItem { Name = "bulbasaur", Url = "https://pokeapi.co/api/v2/pokemon/1/" };

        // Assert
        Assert.Equal("bulbasaur", item.Name);
        Assert.Equal("https://pokeapi.co/api/v2/pokemon/1/", item.Url);
    }

    #endregion
}
