using App.Core.Pokemon;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using PokemonEntity = App.Core.Pokemon.Pokemon;

namespace App.Api.Tests.Todo;

/// <summary>
/// Unit tests for PokemonEndpoints to improve API layer test coverage.
/// Tests all endpoint methods with various scenarios including success and error cases.
/// </summary>
public class PokemonEndpointsTests
{
    private readonly Mock<IPokemonGateway> _gatewayMock;
    private readonly Mock<ILogger<PokemonService>> _serviceLoggerMock;
    private readonly PokemonService _pokemonService;
    private readonly Mock<ILogger<App.Api.Pokemon.PokemonEndpoints>> _loggerMock;
    private readonly App.Api.Pokemon.PokemonEndpoints _pokemonEndpoints;

    public PokemonEndpointsTests()
    {
        // Create mocks
        _gatewayMock = new Mock<IPokemonGateway>();
        _serviceLoggerMock = new Mock<ILogger<PokemonService>>();

        // Create real service with mocked dependencies
        _pokemonService = new PokemonService(_gatewayMock.Object, _serviceLoggerMock.Object);
        _loggerMock = new Mock<ILogger<App.Api.Pokemon.PokemonEndpoints>>();
        _pokemonEndpoints = new App.Api.Pokemon.PokemonEndpoints(
            _pokemonService,
            _loggerMock.Object
        );
    }

    #region FetchPokemonListAsync Tests

    [Fact]
    public async Task FetchPokemonListAsync_WithValidPokemonList_ShouldReturnOkWithResults()
    {
        // Arrange
        var expectedPokemon = new List<PokemonEntity>
        {
            new() { Name = "bulbasaur", Url = "https://pokeapi.co/api/v2/pokemon/1/" },
            new() { Name = "ivysaur", Url = "https://pokeapi.co/api/v2/pokemon/2/" },
            new() { Name = "venusaur", Url = "https://pokeapi.co/api/v2/pokemon/3/" },
        };

        _gatewayMock.Setup(x => x.FetchPokemonListAsync(20, 0)).ReturnsAsync(expectedPokemon);

        // Act
        var result = await _pokemonEndpoints.FetchPokemonListAsync(20, 0);

        // Assert
        var okResult = Assert.IsType<Ok<App.Api.Pokemon.PokemonListResponse>>(result);
        Assert.NotNull(okResult.Value);
        Assert.Equal(3, okResult.Value.Count);
        Assert.NotNull(okResult.Value.Results);
        var resultList = okResult.Value.Results.ToList();
        Assert.Equal("bulbasaur", resultList[0].Name);

        _gatewayMock.Verify(x => x.FetchPokemonListAsync(20, 0), Times.Once);
    }

    [Fact]
    public async Task FetchPokemonListAsync_WithPaginationParams_ShouldPassParametersCorrectly()
    {
        // Arrange
        var expectedPokemon = new List<PokemonEntity>
        {
            new() { Name = "charmander", Url = "https://pokeapi.co/api/v2/pokemon/4/" },
        };

        _gatewayMock.Setup(x => x.FetchPokemonListAsync(10, 50)).ReturnsAsync(expectedPokemon);

        // Act
        var result = await _pokemonEndpoints.FetchPokemonListAsync(10, 50);

        // Assert
        var okResult = Assert.IsType<Ok<App.Api.Pokemon.PokemonListResponse>>(result);
        Assert.NotNull(okResult.Value);
        Assert.Equal(1, okResult.Value.Count);

        _gatewayMock.Verify(x => x.FetchPokemonListAsync(10, 50), Times.Once);
    }

    [Fact]
    public async Task FetchPokemonListAsync_WithNullResponse_ShouldReturnBadGateway()
    {
        // Arrange
        _gatewayMock
            .Setup(x => x.FetchPokemonListAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync((IEnumerable<PokemonEntity>?)null);

        // Act
        var result = await _pokemonEndpoints.FetchPokemonListAsync();

        // Assert
        var problemResult = Assert.IsType<ProblemHttpResult>(result);
        Assert.Equal(StatusCodes.Status502BadGateway, problemResult.StatusCode);
    }

    [Fact]
    public async Task FetchPokemonListAsync_WithEmptyList_ShouldReturnOkWithDefaultPagination()
    {
        // Arrange
        _gatewayMock
            .Setup(x => x.FetchPokemonListAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(new List<PokemonEntity>());

        // Act
        var result = await _pokemonEndpoints.FetchPokemonListAsync(20, 0);

        // Assert
        var okResult = Assert.IsType<Ok<App.Api.Pokemon.PokemonListResponse>>(result);
        Assert.NotNull(okResult.Value);
        Assert.Equal(0, okResult.Value.Count);
        // Next should be null since count is 0 (not equal to limit)
        Assert.Null(okResult.Value.Next);
        // Previous should be null since offset is 0
        Assert.Null(okResult.Value.Previous);
    }

    [Fact]
    public async Task FetchPokemonListAsync_WithFullPage_ShouldIncludeNextLink()
    {
        // Arrange
        var pokemonList = Enumerable
            .Range(1, 20)
            .Select(i => new PokemonEntity
            {
                Name = $"pokemon{i}",
                Url = $"https://pokeapi.co/api/v2/pokemon/{i}/",
            })
            .ToList();

        _gatewayMock.Setup(x => x.FetchPokemonListAsync(20, 0)).ReturnsAsync(pokemonList);

        // Act
        var result = await _pokemonEndpoints.FetchPokemonListAsync(20, 0);

        // Assert
        var okResult = Assert.IsType<Ok<App.Api.Pokemon.PokemonListResponse>>(result);
        Assert.NotNull(okResult.Value);
        Assert.NotNull(okResult.Value.Next);
        Assert.Contains("offset=20", okResult.Value.Next);
        Assert.Null(okResult.Value.Previous); // offset is 0
    }

    [Fact]
    public async Task FetchPokemonListAsync_WithOffsetAndFullPage_ShouldIncludePreviousLink()
    {
        // Arrange
        var pokemonList = Enumerable
            .Range(21, 20)
            .Select(i => new PokemonEntity
            {
                Name = $"pokemon{i}",
                Url = $"https://pokeapi.co/api/v2/pokemon/{i}/",
            })
            .ToList();

        _gatewayMock.Setup(x => x.FetchPokemonListAsync(20, 20)).ReturnsAsync(pokemonList);

        // Act
        var result = await _pokemonEndpoints.FetchPokemonListAsync(20, 20);

        // Assert
        var okResult = Assert.IsType<Ok<App.Api.Pokemon.PokemonListResponse>>(result);
        Assert.NotNull(okResult.Value);
        Assert.NotNull(okResult.Value.Previous);
        Assert.Contains("offset=0", okResult.Value.Previous);
        Assert.NotNull(okResult.Value.Next);
        Assert.Contains("offset=40", okResult.Value.Next);
    }

    #endregion

    #region FetchPokemonByIdAsync Tests

    [Fact]
    public async Task FetchPokemonByIdAsync_WithValidId_ShouldReturnOkWithPokemon()
    {
        // Arrange
        var expectedPokemon = new PokemonEntity
        {
            Name = "pikachu",
            Url = "https://pokeapi.co/api/v2/pokemon/25/",
        };

        _gatewayMock.Setup(x => x.FetchPokemonByIdAsync(25)).ReturnsAsync(expectedPokemon);

        // Act
        var result = await _pokemonEndpoints.FetchPokemonByIdAsync(25);

        // Assert
        var okResult = Assert.IsType<Ok<App.Api.Pokemon.PokemonResponse>>(result);
        Assert.NotNull(okResult.Value);
        Assert.Equal("pikachu", okResult.Value.Name);
        Assert.Equal("https://pokeapi.co/api/v2/pokemon/25/", okResult.Value.Url);

        _gatewayMock.Verify(x => x.FetchPokemonByIdAsync(25), Times.Once);
    }

    [Fact]
    public async Task FetchPokemonByIdAsync_WithInvalidId_ShouldReturnNotFound()
    {
        // Arrange
        _gatewayMock
            .Setup(x => x.FetchPokemonByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((PokemonEntity?)null);

        // Act
        var result = await _pokemonEndpoints.FetchPokemonByIdAsync(99999);

        // Assert
        var statusCodeResult = Assert.IsAssignableFrom<IStatusCodeHttpResult>(result);
        Assert.Equal(StatusCodes.Status404NotFound, statusCodeResult.StatusCode);
    }

    [Fact]
    public async Task FetchPokemonByIdAsync_WithSmallId_ShouldReturnOk()
    {
        // Arrange
        var expectedPokemon = new PokemonEntity
        {
            Name = "bulbasaur",
            Url = "https://pokeapi.co/api/v2/pokemon/1/",
        };

        _gatewayMock.Setup(x => x.FetchPokemonByIdAsync(1)).ReturnsAsync(expectedPokemon);

        // Act
        var result = await _pokemonEndpoints.FetchPokemonByIdAsync(1);

        // Assert
        var okResult = Assert.IsType<Ok<App.Api.Pokemon.PokemonResponse>>(result);
        Assert.NotNull(okResult.Value);
        Assert.Equal("bulbasaur", okResult.Value.Name);
    }

    [Fact]
    public async Task FetchPokemonByIdAsync_WithLargeValidId_ShouldReturnOk()
    {
        // Arrange
        var expectedPokemon = new PokemonEntity
        {
            Name = "arceus",
            Url = "https://pokeapi.co/api/v2/pokemon/493/",
        };

        _gatewayMock.Setup(x => x.FetchPokemonByIdAsync(493)).ReturnsAsync(expectedPokemon);

        // Act
        var result = await _pokemonEndpoints.FetchPokemonByIdAsync(493);

        // Assert
        var okResult = Assert.IsType<Ok<App.Api.Pokemon.PokemonResponse>>(result);
        Assert.NotNull(okResult.Value);
        Assert.Equal("arceus", okResult.Value.Name);
        Assert.Equal("https://pokeapi.co/api/v2/pokemon/493/", okResult.Value.Url);
    }

    [Fact]
    public async Task FetchPokemonByIdAsync_WhenServiceThrowsArgumentException_ShouldRethrow()
    {
        // Arrange
        _gatewayMock
            .Setup(x => x.FetchPokemonByIdAsync(1))
            .ThrowsAsync(new ArgumentException("Invalid ID"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            _pokemonEndpoints.FetchPokemonByIdAsync(1)
        );
        Assert.Contains("Invalid ID", exception.Message);
    }

    #endregion
}
