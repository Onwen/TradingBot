using Microsoft.Extensions.Logging;
using Moq;
using TradingBot.Domain.Enum;
using TradingBot.Domain.Model;
using TradingBot.Domain.Provider;
using TradingBot.Domain.Repository.Position;
using TradingBot.Domain.Repository.Ticker;
using TradingBot.UseCases.Services;

namespace TradingBot.UseCases.Tests.Service;

public class CoinSpotExchangeServiceTests
{
    private readonly Mock<IPositionRepository> _mockPositionRepository;
    private readonly Mock<ITickerRepository> _mockTickerRepository;
    private readonly Mock<IExchangeProvider> _mockExchangeProvider;
    private readonly Mock<ILogger<CoinSpotExchangeService>> _mockLogger;
    private readonly CoinSpotExchangeService _coinSpotExchangeService;
    public CoinSpotExchangeServiceTests()
    {
        _mockPositionRepository = new Mock<IPositionRepository>();
        _mockTickerRepository = new Mock<ITickerRepository>();
        _mockExchangeProvider = new Mock<IExchangeProvider>();
        _mockLogger = new Mock<ILogger<CoinSpotExchangeService>>();
        _coinSpotExchangeService = new CoinSpotExchangeService(_mockPositionRepository.Object, _mockTickerRepository.Object, _mockExchangeProvider.Object, _mockLogger.Object);
    }
    
    #region GetTickersAsync
    [Fact]
    public async void GetTickersAsync_Success()
    {
        // Arrange
        var tickers = new List<PriceSnapshotModel>
        {
            new() { Name = "BTC", Currency = Currency.AUD, Ask = 1000, Bid = 1001, Last = 1002 },
            new() { Name = "ETH", Currency = Currency.AUD, Ask = 2000, Bid = 2001, Last = 2002 },
            new() { Name = "XRP", Currency = Currency.AUD, Ask = 3000, Bid = 3001, Last = 3002 }
        };
        _mockExchangeProvider.Setup(x => x.GetTickers()).ReturnsAsync(tickers);
        _mockTickerRepository.Setup(x => x.SaveTickers(It.IsAny<List<PriceSnapshotDto>>())).Returns(true);
        // Act
        var result = await _coinSpotExchangeService.GetTickersAsync();
        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Count);
        Assert.Equal("BTC", result[0].Name);
        Assert.Equal("ETH", result[1].Name);
        Assert.Equal("XRP", result[2].Name);
        _mockExchangeProvider.Verify(x => x.GetTickers(), Times.Once);
        _mockTickerRepository.Verify(x => x.SaveTickers(It.IsAny<List<PriceSnapshotDto>>()), Times.Once);
    }
    // Where exchange provider returns empty list we should not throw error and not call SaveTickers
    [Fact]
    public async void GetTickersAsync_EmptyList()
    {
        // Arrange
        var tickers = new List<PriceSnapshotModel>();
        _mockExchangeProvider.Setup(x => x.GetTickers()).ReturnsAsync(tickers);
        // Act
        var result = await _coinSpotExchangeService.GetTickersAsync();
        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
        _mockExchangeProvider.Verify(x => x.GetTickers(), Times.Once);
        _mockTickerRepository.Verify(x => x.SaveTickers(It.IsAny<List<PriceSnapshotDto>>()), Times.Never);
    }
    
    // Where exchange provider throws exception we should throw error and not call SaveTickers
    [Fact]
    public async void GetTickersAsync_ExchangeProviderThrowsException()
    {
        // Arrange
        _mockExchangeProvider.Setup(x => x.GetTickers()).ReturnsAsync(() => throw new Exception());
        // Act
        await Assert.ThrowsAnyAsync<Exception>(() => _coinSpotExchangeService.GetTickersAsync());
        // Assert
        _mockExchangeProvider.Verify(x => x.GetTickers(), Times.Once);
        _mockTickerRepository.Verify(x => x.SaveTickers(It.IsAny<List<PriceSnapshotDto>>()), Times.Never);
    }
    
    [Fact]
    public async void GetTickersAsync_RepositoryFails()
    {
        // Arrange
        var tickers = new List<PriceSnapshotModel>
        {
            new() { Name = "BTC", Currency = Currency.AUD, Ask = 1000, Bid = 1001, Last = 1002 },
            new() { Name = "ETH", Currency = Currency.AUD, Ask = 2000, Bid = 2001, Last = 2002 },
            new() { Name = "XRP", Currency = Currency.AUD, Ask = 3000, Bid = 3001, Last = 3002 }
        };
        _mockExchangeProvider.Setup(x => x.GetTickers()).ReturnsAsync(tickers);
        _mockTickerRepository.Setup(x => x.SaveTickers(It.IsAny<List<PriceSnapshotDto>>())).Returns(false);
        // Act
        await Assert.ThrowsAnyAsync<Exception>(() => _coinSpotExchangeService.GetTickersAsync());
        // Assert
        _mockExchangeProvider.Verify(x => x.GetTickers(), Times.Once);
        _mockTickerRepository.Verify(x => x.SaveTickers(It.IsAny<List<PriceSnapshotDto>>()), Times.Once);
    }
    #endregion
    #region GetPortfoliosAsync
    [Fact]
    public async void GetPortfoliosAsync_Success()
    {
        // Arrange
        var positions = new List<PositionModel>
        {
            new() { Name = "BTC", Quantity = 1 },
            new() { Name = "ETH", Quantity = 2 },
            new() { Name = "XRP", Quantity = 3 }
        };
        _mockExchangeProvider.Setup(x => x.GetPortfolio()).ReturnsAsync(positions);
        _mockPositionRepository.Setup(x => x.SavePositions(It.IsAny<string>(), It.IsAny<List<PositionDto>>())).Returns(true);
        // Act
        var result = await _coinSpotExchangeService.GetPortfoliosAsync();
        // Assert
        Assert.NotNull(result);
        Assert.Equal("CoinSpot", result.Exchange);
        Assert.Equal(3, result.Positions.Count);
        Assert.Equal("BTC", result.Positions[0].Name);
        Assert.Equal("ETH", result.Positions[1].Name);
        Assert.Equal("XRP", result.Positions[2].Name);
        _mockExchangeProvider.Verify(x => x.GetPortfolio(), Times.Once);
        _mockPositionRepository.Verify(x => x.SavePositions(It.IsAny<string>(), It.IsAny<List<PositionDto>>()), Times.Once);
    }
    
    [Fact]
    public async void GetPortfoliosAsync_ExchangeProviderThrowsException()
    {
        // Arrange
        _mockExchangeProvider.Setup(x => x.GetPortfolio()).ReturnsAsync(() => throw new Exception());
        // Act
        await Assert.ThrowsAnyAsync<Exception>(() => _coinSpotExchangeService.GetPortfoliosAsync());
        // Assert
        _mockExchangeProvider.Verify(x => x.GetPortfolio(), Times.Once);
        _mockPositionRepository.Verify(x => x.SavePositions(It.IsAny<string>(), It.IsAny<List<PositionDto>>()), Times.Never);
    }
    
    [Fact]
    public async void GetPortfoliosAsync_RepositoryFails()
    {
        // Arrange
        var positions = new List<PositionModel>
        {
            new() { Name = "BTC", Quantity = 1 },
            new() { Name = "ETH", Quantity = 2 },
            new() { Name = "XRP", Quantity = 3 }
        };
        _mockExchangeProvider.Setup(x => x.GetPortfolio()).ReturnsAsync(positions);
        _mockPositionRepository.Setup(x => x.SavePositions(It.IsAny<string>(), It.IsAny<List<PositionDto>>())).Returns(false);
        // Act
        await Assert.ThrowsAnyAsync<Exception>(() => _coinSpotExchangeService.GetPortfoliosAsync());
        // Assert
        _mockExchangeProvider.Verify(x => x.GetPortfolio(), Times.Once);
        _mockPositionRepository.Verify(x => x.SavePositions(It.IsAny<string>(), It.IsAny<List<PositionDto>>()), Times.Once);
    }
    #endregion
    #region BuyAsync
    [Fact]
    public async void BuyAsync_Success()
    {
        // Arrange
        var position = new PositionModel { Name = "BTC", Quantity = 1 };
        _mockExchangeProvider.Setup(x => x.Buy(It.IsAny<string>(), It.IsAny<decimal>())).ReturnsAsync(position);
        _mockPositionRepository.Setup(x => x.SavePosition(It.IsAny<string>(), It.IsAny<PositionDto>())).Returns(true);
        // Act
        var result = await _coinSpotExchangeService.BuyAsync("BTC", 1);
        // Assert
        Assert.NotNull(result);
        Assert.Equal("BTC", result.Name);
        Assert.Equal(1, result.Quantity);
        _mockExchangeProvider.Verify(x => x.Buy(It.IsAny<string>(), It.IsAny<decimal>()), Times.Once);
        _mockPositionRepository.Verify(x => x.SavePosition(It.IsAny<string>(), It.IsAny<PositionDto>()), Times.Once);
    }
    
    [Fact]
    public async void BuyAsync_ExchangeProviderThrowsException()
    {
        // Arrange
        _mockExchangeProvider.Setup(x => x.Buy(It.IsAny<string>(), It.IsAny<decimal>())).ReturnsAsync(() => throw new Exception());
        // Act
        await Assert.ThrowsAnyAsync<Exception>(() => _coinSpotExchangeService.BuyAsync("BTC", 1));
        // Assert
        _mockExchangeProvider.Verify(x => x.Buy(It.IsAny<string>(), It.IsAny<decimal>()), Times.Once);
        _mockPositionRepository.Verify(x => x.SavePosition(It.IsAny<string>(), It.IsAny<PositionDto>()), Times.Never);
    }
    
    [Fact]
    public async void BuyAsync_RepositoryFails()
    {
        // Arrange
        var position = new PositionModel { Name = "BTC", Quantity = 1 };
        _mockExchangeProvider.Setup(x => x.Buy(It.IsAny<string>(), It.IsAny<decimal>())).ReturnsAsync(position);
        _mockPositionRepository.Setup(x => x.SavePosition(It.IsAny<string>(), It.IsAny<PositionDto>())).Returns(false);
        // Act
        await Assert.ThrowsAnyAsync<Exception>(() => _coinSpotExchangeService.BuyAsync("BTC", 1));
        // Assert
        _mockExchangeProvider.Verify(x => x.Buy(It.IsAny<string>(), It.IsAny<decimal>()), Times.Once);
        _mockPositionRepository.Verify(x => x.SavePosition(It.IsAny<string>(), It.IsAny<PositionDto>()), Times.Once);
    }
    #endregion
    #region SellAsync
    [Fact]
    public async void SellAsync_Success()
    {
        // Arrange
        var position = new PositionModel { Name = "BTC", Quantity = 1 };
        _mockExchangeProvider.Setup(x => x.Sell(It.IsAny<string>(), It.IsAny<decimal>())).ReturnsAsync(position);
        _mockPositionRepository.Setup(x => x.SavePosition(It.IsAny<string>(), It.IsAny<PositionDto>())).Returns(true);
        // Act
        var result = await _coinSpotExchangeService.SellAsync("BTC", 1);
        // Assert
        Assert.NotNull(result);
        Assert.Equal("BTC", result.Name);
        Assert.Equal(1, result.Quantity);
        _mockExchangeProvider.Verify(x => x.Sell(It.IsAny<string>(), It.IsAny<decimal>()), Times.Once);
        _mockPositionRepository.Verify(x => x.SavePosition(It.IsAny<string>(), It.IsAny<PositionDto>()), Times.Once);
    }
    
    [Fact]
    public async void SellAsync_ExchangeProviderThrowsException()
    {
        // Arrange
        _mockExchangeProvider.Setup(x => x.Sell(It.IsAny<string>(), It.IsAny<decimal>())).ReturnsAsync(() => throw new Exception());
        // Act
        await Assert.ThrowsAnyAsync<Exception>(() => _coinSpotExchangeService.SellAsync("BTC", 1));
        // Assert
        _mockExchangeProvider.Verify(x => x.Sell(It.IsAny<string>(), It.IsAny<decimal>()), Times.Once);
        _mockPositionRepository.Verify(x => x.SavePosition(It.IsAny<string>(), It.IsAny<PositionDto>()), Times.Never);
    }
    
    [Fact]
    public async void SellAsync_RepositoryFails()
    {
        // Arrange
        var position = new PositionModel { Name = "BTC", Quantity = 1 };
        _mockExchangeProvider.Setup(x => x.Sell(It.IsAny<string>(), It.IsAny<decimal>())).ReturnsAsync(position);
        _mockPositionRepository.Setup(x => x.SavePosition(It.IsAny<string>(), It.IsAny<PositionDto>())).Returns(false);
        // Act
        await Assert.ThrowsAnyAsync<Exception>(() => _coinSpotExchangeService.SellAsync("BTC", 1));
        // Assert
        _mockExchangeProvider.Verify(x => x.Sell(It.IsAny<string>(), It.IsAny<decimal>()), Times.Once);
        _mockPositionRepository.Verify(x => x.SavePosition(It.IsAny<string>(), It.IsAny<PositionDto>()), Times.Once);
    }
    #endregion
}