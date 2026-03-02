using App.Core.Poke;

using Microsoft.Extensions.Logging;

using Moq;

using Xunit;

namespace App.Core.Tests.Pokemon;

public class PokemonServiceTests
{
    private readonly Mock<IPokemonGateway> _gatewayMock;
    private readonly Mock<ILogger<PokemonService>> _loggerMock;
    private readonly PokemonService _service;

    public PokemonServiceTests()
    {
        _gatewayMock = new Mock<IPokemonGateway>();
        _loggerMock = new Mock<ILogger<PokemonService>>();
        _service = new PokemonService(_gatewayMock.Object, _loggerMock.Object);
    }

    #region GetPokemonListAsync Tests

    [Fact]
    public async Task GetPokemonListAsync_WithValidParams_ShouldReturnList()
    {
        // Arrange
        var expected = new List<App.Core.Poke.Pokemon>
        {
            new() { Name = "bulbasaur", Url = new Uri("https://pokeapi.co/api/v2/pokemon/1/", UriKind.RelativeOrAbsolute) },
            new() { Name = "charmander", Url = new Uri("https://pokeapi.co/api/v2/pokemon/4/", UriKind.RelativeOrAbsolute) },
        };

        _gatewayMock.Setup(x => x.FetchPokemonListAsync(20, 0)).ReturnsAsync(expected);

        // Act
        var result = await _service.GetPokemonListAsync(20, 0);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        _gatewayMock.Verify(x => x.FetchPokemonListAsync(20, 0), Times.Once);
    }

    [Fact]
    public async Task GetPokemonListAsync_WithNegativeLimit_ShouldClampTo1()
    {
        // Arrange
        _gatewayMock
            .Setup(x => x.FetchPokemonListAsync(1, 0))
            .ReturnsAsync(new List<App.Core.Poke.Pokemon>());

        // Act
        await _service.GetPokemonListAsync(-5, 0);

        // Assert - the service should have corrected the limit to 1
        _gatewayMock.Verify(x => x.FetchPokemonListAsync(1, 0), Times.Once);
    }

    [Fact]
    public async Task GetPokemonListAsync_WithZeroLimit_ShouldClampTo1()
    {
        // Arrange
        _gatewayMock
            .Setup(x => x.FetchPokemonListAsync(1, 0))
            .ReturnsAsync(new List<App.Core.Poke.Pokemon>());

        // Act
        await _service.GetPokemonListAsync(0, 0);

        // Assert
        _gatewayMock.Verify(x => x.FetchPokemonListAsync(1, 0), Times.Once);
    }

    [Fact]
    public async Task GetPokemonListAsync_WithLimitOver100_ShouldCapAt100()
    {
        // Arrange
        _gatewayMock
            .Setup(x => x.FetchPokemonListAsync(100, 0))
            .ReturnsAsync(new List<App.Core.Poke.Pokemon>());

        // Act
        await _service.GetPokemonListAsync(200, 0);

        // Assert - the service should have capped the limit to 100
        _gatewayMock.Verify(x => x.FetchPokemonListAsync(100, 0), Times.Once);
    }

    [Fact]
    public async Task GetPokemonListAsync_WithNegativeOffset_ShouldDefaultTo0()
    {
        // Arrange
        _gatewayMock
            .Setup(x => x.FetchPokemonListAsync(20, 0))
            .ReturnsAsync(new List<App.Core.Poke.Pokemon>());

        // Act
        await _service.GetPokemonListAsync(20, -10);

        // Assert - the service should have corrected the offset to 0
        _gatewayMock.Verify(x => x.FetchPokemonListAsync(20, 0), Times.Once);
    }

    [Fact]
    public async Task GetPokemonListAsync_WhenGatewayReturnsNull_ShouldReturnNull()
    {
        // Arrange
        _gatewayMock
            .Setup(x => x.FetchPokemonListAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync((IEnumerable<App.Core.Poke.Pokemon>?)null);

        // Act
        var result = await _service.GetPokemonListAsync();

        // Assert
        Assert.Null(result);
    }

    #endregion

    #region GetPokemonByIdAsync Tests

    [Fact]
    public async Task GetPokemonByIdAsync_WithValidId_ShouldReturnPokemon()
    {
        // Arrange
        var expected = new App.Core.Poke.Pokemon
        {
            Name = "pikachu", Url = new Uri("https://pokeapi.co/api/v2/pokemon/25/", UriKind.RelativeOrAbsolute),
        };

        _gatewayMock.Setup(x => x.FetchPokemonByIdAsync(25)).ReturnsAsync(expected);

        // Act
        var result = await _service.GetPokemonByIdAsync(25);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("pikachu", result.Name);
    }

    [Fact]
    public async Task GetPokemonByIdAsync_WithZeroId_ShouldThrowArgumentException()
    {
        // Act & Assert
        var ex = await Assert.ThrowsAsync<ArgumentException>(() => _service.GetPokemonByIdAsync(0));

        Assert.Contains("Pokemon ID must be greater than zero", ex.Message, StringComparison.OrdinalIgnoreCase);
        _gatewayMock.Verify(x => x.FetchPokemonByIdAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task GetPokemonByIdAsync_WithNegativeId_ShouldThrowArgumentException()
    {
        // Act & Assert
        var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.GetPokemonByIdAsync(-1)
        );

        Assert.Contains("Pokemon ID must be greater than zero", ex.Message, StringComparison.OrdinalIgnoreCase);
        _gatewayMock.Verify(x => x.FetchPokemonByIdAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task GetPokemonByIdAsync_WhenGatewayReturnsNull_ShouldReturnNull()
    {
        // Arrange
        _gatewayMock
            .Setup(x => x.FetchPokemonByIdAsync(999))
            .ReturnsAsync((App.Core.Poke.Pokemon?)null);

        // Act
        var result = await _service.GetPokemonByIdAsync(999);

        // Assert
        Assert.Null(result);
    }

    #endregion

    #region Constructor Tests

    [Fact]
    public void Constructor_WithNullGateway_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new PokemonService(null!, _loggerMock.Object)
        );
    }

    [Fact]
    public void Constructor_WithNullLogger_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new PokemonService(_gatewayMock.Object, null!)
        );
    }

    #endregion
}
