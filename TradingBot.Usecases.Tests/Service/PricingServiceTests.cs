using Microsoft.Extensions.Logging;
using Moq;
using TradingBot.Domain.Enum;
using TradingBot.Domain.Mapping;
using TradingBot.Domain.Model;
using TradingBot.Domain.Provider;
using TradingBot.Domain.Repository.Order;
using TradingBot.Domain.Repository.Position;
using TradingBot.Domain.Repository.PositionTargetWeighting;
using TradingBot.Domain.Repository.PriceHistory;
using TradingBot.Domain.Repository.Return;
using TradingBot.Domain.Repository.StrategyLog;
using TradingBot.Domain.Repository.Ticker;
using TradingBot.Domain.Service;
using TradingBot.Domain.TimeProvider;
using TradingBot.UseCases.Services;

namespace TradingBot.UseCases.Tests.Service;

public class PricingServiceTests
{
    private readonly Mock<IPositionSnapshotRepository> _mockTickerRepository;
    private readonly Mock<IPriceHistoryRepository> _mockPriceHistoryRepository;
    private readonly Mock<IExchangeProvider> _mockExchangeProvider;
    private readonly TimeProvider _timeProvider;
    private readonly Mock<ILogger<PricingService>> _mockLogger;
    private readonly PricingService _pricingService;
    private readonly DateTime now = DateTimeOffset.UtcNow.DateTime;
    public PricingServiceTests()
    {
        _mockTickerRepository = new Mock<IPositionSnapshotRepository>();
        _mockPriceHistoryRepository = new Mock<IPriceHistoryRepository>();
        _mockExchangeProvider = new Mock<IExchangeProvider>();
        _timeProvider = new StaticTimeProvider(now);
        _mockLogger = new Mock<ILogger<PricingService>>();
        _pricingService = new PricingService(_mockTickerRepository.Object, _mockPriceHistoryRepository.Object, _mockExchangeProvider.Object, _timeProvider, _mockLogger.Object);
    }
    
    #region GetPriceSnapshotsAsync
    [Fact]
    public async void GetTickersAsync_Success()
    {
        // Arrange
        var tickers = new List<PriceSnapshotModel>
        {
            new() { Name = Coin.BTC, Currency = Currency.AUD, Ask = 1000, Bid = 1001, Last = 1002 },
            new() { Name = Coin.ETH, Currency = Currency.AUD, Ask = 2000, Bid = 2001, Last = 2002 },
            new() { Name = Coin.XRP, Currency = Currency.AUD, Ask = 3000, Bid = 3001, Last = 3002 }
        };
        _mockExchangeProvider.Setup(x => x.GetPriceSnapshots()).ReturnsAsync(tickers);
        _mockTickerRepository.Setup(x => x.SavePriceSnapshots(It.IsAny<List<PriceSnapshotDto>>())).Returns(true);
        // Act
        var result = await _pricingService.GetPriceSnapshotsAsync();
        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Count);
        Assert.Equal(Coin.BTC, result[0].Name);
        Assert.Equal(Coin.ETH, result[1].Name);
        Assert.Equal(Coin.XRP, result[2].Name);
        _mockExchangeProvider.Verify(x => x.GetPriceSnapshots(), Times.Once);
        _mockTickerRepository.Verify(x => x.SavePriceSnapshots(It.IsAny<List<PriceSnapshotDto>>()), Times.Once);
    }
    // Where exchange provider returns empty list we should not throw error and not call SavePriceSnapshots
    [Fact]
    public async void GetTickersAsync_EmptyList()
    {
        // Arrange
        var tickers = new List<PriceSnapshotModel>();
        _mockExchangeProvider.Setup(x => x.GetPriceSnapshots()).ReturnsAsync(tickers);
        // Act
        var result = await _pricingService.GetPriceSnapshotsAsync();
        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
        _mockExchangeProvider.Verify(x => x.GetPriceSnapshots(), Times.Once);
        _mockTickerRepository.Verify(x => x.SavePriceSnapshots(It.IsAny<List<PriceSnapshotDto>>()), Times.Never);
    }
    
    // Where exchange provider throws exception we should throw error and not call SavePriceSnapshots
    [Fact]
    public async void GetTickersAsync_ExchangeProviderThrowsException()
    {
        // Arrange
        _mockExchangeProvider.Setup(x => x.GetPriceSnapshots()).ReturnsAsync(() => throw new Exception());
        // Act
        await Assert.ThrowsAnyAsync<Exception>(() => _pricingService.GetPriceSnapshotsAsync());
        // Assert
        _mockExchangeProvider.Verify(x => x.GetPriceSnapshots(), Times.Once);
        _mockTickerRepository.Verify(x => x.SavePriceSnapshots(It.IsAny<List<PriceSnapshotDto>>()), Times.Never);
    }
    
    [Fact]
    public async void GetTickersAsync_RepositoryFails()
    {
        // Arrange
        var tickers = new List<PriceSnapshotModel>
        {
            new() { Name = Coin.BTC, Currency = Currency.AUD, Ask = 1000, Bid = 1001, Last = 1002 },
            new() { Name = Coin.ETH, Currency = Currency.AUD, Ask = 2000, Bid = 2001, Last = 2002 },
            new() { Name = Coin.XRP, Currency = Currency.AUD, Ask = 3000, Bid = 3001, Last = 3002 }
        };
        _mockExchangeProvider.Setup(x => x.GetPriceSnapshots()).ReturnsAsync(tickers);
        _mockTickerRepository.Setup(x => x.SavePriceSnapshots(It.IsAny<List<PriceSnapshotDto>>())).Returns(false);
        // Act
        await Assert.ThrowsAnyAsync<Exception>(() => _pricingService.GetPriceSnapshotsAsync());
        // Assert
        _mockExchangeProvider.Verify(x => x.GetPriceSnapshots(), Times.Once);
        _mockTickerRepository.Verify(x => x.SavePriceSnapshots(It.IsAny<List<PriceSnapshotDto>>()), Times.Once);
    }
    #endregion
    #region GetDailyPricesAsync
    // Test GetDailyPricesAsync
    [Fact]
    public async void GetDailyPricesAsync_Success()
    {
        // Arrange
        var prices = new List<PriceHistoryDto>
        {
            new("Bitcoin", Coin.BTC, now, 1000, now, 1001, now, 1002, now, 1003, 1004, 1005),
            new("Ethereum", Coin.ETH, now, 2000, now, 2001, now, 2002, now, 2003, 2004, 2005)
        };
        _mockPriceHistoryRepository.Setup(x => x.GetPriceHistory(It.IsAny<DateTime>(), It.IsAny<DateTime>())).ReturnsAsync(prices);
        // Act
        var result = await _pricingService.GetDailyPricesAsync(DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow);
        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Equal(Coin.BTC, result[0].Symbol);
        Assert.Equal(Coin.ETH, result[1].Symbol);
        _mockPriceHistoryRepository.Verify(x => x.GetPriceHistory(It.IsAny<DateTime>(), It.IsAny<DateTime>()), Times.Once);
    }
    // Where repository throws exception we should throw error
    [Fact]
    public async void GetDailyPricesAsync_RepositoryThrowsException()
    {
        // Arrange
        _mockPriceHistoryRepository.Setup(x => x.GetPriceHistory(It.IsAny<DateTime>(), It.IsAny<DateTime>())).ReturnsAsync(() => throw new Exception());
        // Act
        await Assert.ThrowsAnyAsync<Exception>(() => _pricingService.GetDailyPricesAsync(DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow));
        // Assert
        _mockPriceHistoryRepository.Verify(x => x.GetPriceHistory(It.IsAny<DateTime>(), It.IsAny<DateTime>()), Times.Once);
    }
    // Where repository returns empty list we should not throw error
    [Fact]
    public async void GetDailyPricesAsync_EmptyList()
    {
        // Arrange
        var prices = new List<PriceHistoryDto>();
        _mockPriceHistoryRepository.Setup(x => x.GetPriceHistory(It.IsAny<DateTime>(), It.IsAny<DateTime>())).ReturnsAsync(prices);
        // Act
        var result =
            await _pricingService.GetDailyPricesAsync(DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow);
        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
        _mockPriceHistoryRepository.Verify(x => x.GetPriceHistory(It.IsAny<DateTime>(), It.IsAny<DateTime>()), Times.Once);
    }
    #endregion
}