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
using TradingBot.Domain.TimeProvider;
using TradingBot.UseCases.Services;

namespace TradingBot.UseCases.Tests.Service;

public class CoinSpotExchangeServiceTests
{
    private const string Exchange = "exchange";
    private readonly Mock<IPositionRepository> _mockPositionRepository;
    private readonly Mock<IPositionSnapshotRepository> _mockTickerRepository;
    private readonly Mock<IStrategyLogRepository> _mockStrategyLogRepository;
    private readonly Mock<IPositionTargetWeightingRepository> _mockPositionTargetWeightingRepository;
    private readonly Mock<IReturnRepository> _mockReturnRepository;
    private readonly Mock<IOrderRepository> _mockOrderRepository;
    private readonly Mock<IPriceHistoryRepository> _mockPriceHistoryRepository;
    private readonly Mock<IExchangeProvider> _mockExchangeProvider;
    private readonly TimeProvider _timeProvider;
    private readonly Mock<ILogger<CoinSpotExchangeService>> _mockLogger;
    private readonly CoinSpotExchangeService _coinSpotExchangeService;
    private readonly DateTimeOffset now = DateTimeOffset.UtcNow;
    public CoinSpotExchangeServiceTests()
    {
        _mockPositionRepository = new Mock<IPositionRepository>();
        _mockTickerRepository = new Mock<IPositionSnapshotRepository>();
        _mockStrategyLogRepository = new Mock<IStrategyLogRepository>();
        _mockPositionTargetWeightingRepository = new Mock<IPositionTargetWeightingRepository>();
        _mockReturnRepository = new Mock<IReturnRepository>();
        _mockOrderRepository = new Mock<IOrderRepository>();
        _mockPriceHistoryRepository = new Mock<IPriceHistoryRepository>();
        _mockExchangeProvider = new Mock<IExchangeProvider>();
        _timeProvider = new StaticTimeProvider(now);
        _mockLogger = new Mock<ILogger<CoinSpotExchangeService>>();
        _coinSpotExchangeService = new CoinSpotExchangeService(_mockPositionRepository.Object, _mockTickerRepository.Object, _mockStrategyLogRepository.Object, _mockPositionTargetWeightingRepository.Object, _mockReturnRepository.Object, _mockOrderRepository.Object, _mockPriceHistoryRepository.Object, _mockExchangeProvider.Object, _timeProvider, _mockLogger.Object);
    }
    
    #region MarketBuyAsync
    [Fact]
    public async void BuyAsync_Success()
    {
        // Arrange
        var order = new MarketOrderModel()
        {
            Amount = 1,
            Coin = Coin.BTC,
            Exchange = Exchange,
            Id = "12345678901234567890",
            Market = "BTC/AUD",
            OrderType = "BUY",
            Rate = 123.344m,
            Timestamp = now,
        };
        var completedOrder = new MarketOrderModel()
        {
            Amount = order.Amount,
            Coin = order.Coin,
            Exchange = order.Exchange,
            Id = order.Id,
            Market = order.Market,
            OrderType = order.OrderType,
            Rate = order.Rate,
            Timestamp = order.Timestamp,
            Total = 1,
            AudGst = 2,
            AudTotal = 3,
            SoldDate = now,
            AudFeeExGst = 4
        };
        _mockExchangeProvider.Setup(x => x.MarketBuy(It.IsAny<string>(), It.IsAny<decimal>())).ReturnsAsync(order);
        _mockExchangeProvider.Setup(x => x.GetCompletedMarketOrders()).ReturnsAsync([completedOrder]);
        _mockOrderRepository.Setup(x => x.SaveOrder(It.IsAny<OrderDto>())).Returns(true);
        // Act
        var result = await _coinSpotExchangeService.MarketBuyAsync(Coin.BTC, 1);
        // Assert
        Assert.NotNull(result);
        Assert.Equal(Coin.BTC, result.Coin);
        Assert.Equal(1, result.Amount);
        Assert.Equal(now, result.SoldDate);
        Assert.Equal(1, result.Total);
        Assert.Equal(2, result.AudGst);
        Assert.Equal(3, result.AudTotal);
        Assert.Equal(4, result.AudFeeExGst);
        _mockExchangeProvider.Verify(x => x.MarketBuy(It.IsAny<string>(), It.IsAny<decimal>()), Times.Once);
        _mockExchangeProvider.Verify(x => x.GetCompletedMarketOrders(), Times.Once);
        _mockOrderRepository.Verify(x => x.SaveOrder(It.IsAny<OrderDto>()), Times.Exactly(2));
    }
    
    [Fact]
    public async void BuyAsync_ExchangeProviderThrowsException()
    {
        // Arrange
        _mockExchangeProvider.Setup(x => x.MarketBuy(It.IsAny<string>(), It.IsAny<decimal>())).ReturnsAsync(() => throw new Exception());
        // Act
        await Assert.ThrowsAnyAsync<Exception>(() => _coinSpotExchangeService.MarketBuyAsync(Coin.BTC, 1));
        // Assert
        _mockExchangeProvider.Verify(x => x.MarketBuy(It.IsAny<string>(), It.IsAny<decimal>()), Times.Once);
        _mockPositionRepository.Verify(x => x.SavePosition(It.IsAny<string>(), It.IsAny<PositionDto>()), Times.Never);
    }
    
    [Fact]
    public async void BuyAsync_RepositoryFails()
    {
        // Arrange
        var order = new MarketOrderModel()
        {
            Amount = 1,
            Coin = Coin.BTC,
            Exchange = Exchange,
            Id = "12345678901234567890",
            Market = "BTC/AUD",
            OrderType = "BUY",
            Rate = 123.344m,
            Timestamp = now,
        };
        _mockExchangeProvider.Setup(x => x.MarketBuy(It.IsAny<string>(), It.IsAny<decimal>())).ReturnsAsync(order);
        _mockOrderRepository.Setup(x => x.SaveOrder(It.IsAny<OrderDto>())).Returns(false);
        // Act
        await Assert.ThrowsAnyAsync<Exception>(() => _coinSpotExchangeService.MarketBuyAsync(Coin.BTC, 1));
        // Assert
        _mockExchangeProvider.Verify(x => x.MarketBuy(It.IsAny<string>(), It.IsAny<decimal>()), Times.Once);
        _mockOrderRepository.Verify(x => x.SaveOrder(It.IsAny<OrderDto>()), Times.Once);
    }
    // Test MarketBuyAsync with order never completing results in order being cancelled
    [Fact]
    public async void BuyAsync_NeverCompletes()
    {
        // Arrange
        var order = new MarketOrderModel()
        {
            Amount = 1,
            Coin = Coin.BTC,
            Exchange = Exchange,
            Id = "12345678901234567890",
            Market = "BTC/AUD",
            OrderType = "BUY",
            Rate = 123.344m,
            Timestamp = now,
        };
        _mockExchangeProvider.Setup(x => x.MarketBuy(It.IsAny<string>(), It.IsAny<decimal>())).ReturnsAsync(order);
        _mockExchangeProvider.Setup(x => x.GetCompletedMarketOrders()).ReturnsAsync(new List<MarketOrderModel>());
        _mockOrderRepository.Setup(x => x.SaveOrder(It.IsAny<OrderDto>())).Returns(true);
        // Act
        var result = await _coinSpotExchangeService.MarketBuyAsync(Coin.BTC, 1);
        // Assert
        Assert.NotNull(result);
        Assert.Equal(Coin.BTC, result.Coin);
        Assert.Equal(1, result.Amount);
        Assert.Null(result.SoldDate);
        Assert.Null(result.Total);
        Assert.Null(result.AudGst);
        Assert.Null(result.AudTotal);
        Assert.Null(result.AudFeeExGst);
        _mockExchangeProvider.Verify(x => x.MarketBuy(It.IsAny<string>(), It.IsAny<decimal>()), Times.Once);
        _mockExchangeProvider.Verify(x => x.GetCompletedMarketOrders(), Times.Exactly(10));
        _mockOrderRepository.Verify(x => x.SaveOrder(It.IsAny<OrderDto>()), Times.Exactly(2));
    }
    #endregion
    #region MarketSellAsync
    [Fact]
    public async void SellAsync_Success()
    {
        // Arrange
        var order = new MarketOrderModel()
        {
            Amount = 1,
            Coin = Coin.BTC,
            Exchange = Exchange,
            Id = "12345678901234567890",
            Market = "BTC/AUD",
            OrderType = "SELL",
            Rate = 123.344m,
            Timestamp = now,
        };
        var completedOrder = new MarketOrderModel()
        {
            Amount = order.Amount,
            Coin = order.Coin,
            Exchange = order.Exchange,
            Id = order.Id,
            Market = order.Market,
            OrderType = order.OrderType,
            Rate = order.Rate,
            Timestamp = order.Timestamp,
            Total = 1,
            AudGst = 2,
            AudTotal = 3,
            SoldDate = now,
            AudFeeExGst = 4
        };
        _mockExchangeProvider.Setup(x => x.MarketSell(It.IsAny<string>(), It.IsAny<decimal>())).ReturnsAsync(order);
        _mockExchangeProvider.Setup(x => x.GetCompletedMarketOrders()).ReturnsAsync([completedOrder]);
        _mockOrderRepository.Setup(x => x.SaveOrder(It.IsAny<OrderDto>())).Returns(true);
        // Act
        var result = await _coinSpotExchangeService.MarketSellAsync(Coin.BTC, 1);
        // Assert
        Assert.NotNull(result);
        Assert.Equal(Coin.BTC, result.Coin);
        Assert.Equal(1, result.Amount);
        Assert.Equal(now, result.SoldDate);
        Assert.Equal(1, result.Total);
        Assert.Equal(2, result.AudGst);
        Assert.Equal(3, result.AudTotal);
        Assert.Equal(4, result.AudFeeExGst);
        _mockExchangeProvider.Verify(x => x.MarketSell(It.IsAny<string>(), It.IsAny<decimal>()), Times.Once);
        _mockExchangeProvider.Verify(x => x.GetCompletedMarketOrders(), Times.Once);
        _mockOrderRepository.Verify(x => x.SaveOrder(It.IsAny<OrderDto>()), Times.Exactly(2));
    }
    
    [Fact]
    public async void SellAsync_ExchangeProviderThrowsException()
    {
        // Arrange
        _mockExchangeProvider.Setup(x => x.MarketSell(It.IsAny<string>(), It.IsAny<decimal>())).ReturnsAsync(() => throw new Exception());
        // Act
        await Assert.ThrowsAnyAsync<Exception>(() => _coinSpotExchangeService.MarketSellAsync(Coin.BTC, 1));
        // Assert
        _mockExchangeProvider.Verify(x => x.MarketSell(It.IsAny<string>(), It.IsAny<decimal>()), Times.Once);
        _mockPositionRepository.Verify(x => x.SavePosition(It.IsAny<string>(), It.IsAny<PositionDto>()), Times.Never);
    }
    
    [Fact]
    public async void SellAsync_RepositoryFails()
    {
        // Arrange
        var order = new MarketOrderModel()
        {
            Amount = 1,
            Coin = Coin.BTC,
            Exchange = Exchange,
            Id = "12345678901234567890",
            Market = "BTC/AUD",
            OrderType = "SELL",
            Rate = 123.344m,
            Timestamp = now,
        };
        _mockExchangeProvider.Setup(x => x.MarketSell(It.IsAny<string>(), It.IsAny<decimal>())).ReturnsAsync(order);
        _mockOrderRepository.Setup(x => x.SaveOrder(It.IsAny<OrderDto>())).Returns(false);
        // Act
        await Assert.ThrowsAnyAsync<Exception>(() => _coinSpotExchangeService.MarketSellAsync(Coin.BTC, 1));
        // Assert
        _mockExchangeProvider.Verify(x => x.MarketSell(It.IsAny<string>(), It.IsAny<decimal>()), Times.Once);
        _mockOrderRepository.Verify(x => x.SaveOrder(It.IsAny<OrderDto>()), Times.Once);
    }
    // Test MarketSellAsync with order never completing results in order being cancelled
    [Fact]
    public async void SellAsync_NeverCompletes()
    {
        // Arrange
        var order = new MarketOrderModel()
        {
            Amount = 1,
            Coin = Coin.BTC,
            Exchange = Exchange,
            Id = "12345678901234567890",
            Market = "BTC/AUD",
            OrderType = "SELL",
            Rate = 123.344m,
            Timestamp = now,
        };
        _mockExchangeProvider.Setup(x => x.MarketSell(It.IsAny<string>(), It.IsAny<decimal>())).ReturnsAsync(order);
        _mockExchangeProvider.Setup(x => x.GetCompletedMarketOrders()).ReturnsAsync(new List<MarketOrderModel>());
        _mockOrderRepository.Setup(x => x.SaveOrder(It.IsAny<OrderDto>())).Returns(true);
        // Act
        var result = await _coinSpotExchangeService.MarketSellAsync(Coin.BTC, 1);
        // Assert
        Assert.NotNull(result);
        Assert.Equal(Coin.BTC, result.Coin);
        Assert.Equal(1, result.Amount);
        Assert.Null(result.SoldDate);
        Assert.Null(result.Total);
        Assert.Null(result.AudGst);
        Assert.Null(result.AudTotal);
        Assert.Null(result.AudFeeExGst);
        _mockExchangeProvider.Verify(x => x.MarketSell(It.IsAny<string>(), It.IsAny<decimal>()), Times.Once);
        _mockExchangeProvider.Verify(x => x.GetCompletedMarketOrders(), Times.Exactly(10));
        _mockOrderRepository.Verify(x => x.SaveOrder(It.IsAny<OrderDto>()), Times.Exactly(2));
    }
    #endregion
    #region GetLogsByStrategy
    // Test GetLogsByStrategy
    [Fact]
    public async void GetLogs_Success()
    {
        // Arrange
        var logs = new List<StrategyLogModel>
        {
            new() { StrategyName = "Test 1", Message = "Test", Timestamp = DateTimeOffset.UtcNow },
            new() { StrategyName = "Test 2", Message = "Test", Timestamp = DateTimeOffset.UtcNow },
            new() { StrategyName = "Test 3", Message = "Test", Timestamp = DateTimeOffset.UtcNow }
        };
        _mockStrategyLogRepository.Setup(x => x.GetLogs(It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>())).ReturnsAsync(logs.MapToStrategyLogDto());
        // Act
        var result = await _coinSpotExchangeService.GetLogs(DateTimeOffset.UtcNow, DateTimeOffset.UtcNow);
        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Count);
        Assert.Equal("Test 1", result[0].StrategyName);
        Assert.Equal("Test 2", result[1].StrategyName);
        Assert.Equal("Test 3", result[2].StrategyName);
        _mockStrategyLogRepository.Verify(x => x.GetLogs(It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>()), Times.Once);
    }
    // Where repository throws exception we should throw error
    [Fact]
    public async void GetLogs_RepositoryThrowsException()
    {
        // Arrange
        _mockStrategyLogRepository.Setup(x => x.GetLogs(It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>())).ReturnsAsync(() => throw new Exception());
        // Act
        await Assert.ThrowsAnyAsync<Exception>(() => _coinSpotExchangeService.GetLogs(DateTimeOffset.UtcNow, DateTimeOffset.UtcNow));
        // Assert
        _mockStrategyLogRepository.Verify(x => x.GetLogs(It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>()), Times.Once);
    }
    // Where repository returns empty list we should not throw error
    [Fact]
    public async void GetLogs_EmptyList()
    {
        // Arrange
        var logs = new List<StrategyLogModel>();
        _mockStrategyLogRepository.Setup(x => x.GetLogs(It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>())).ReturnsAsync(logs.MapToStrategyLogDto());
        // Act
        var result = await _coinSpotExchangeService.GetLogs(DateTimeOffset.UtcNow, DateTimeOffset.UtcNow);
        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
        _mockStrategyLogRepository.Verify(x => x.GetLogs(It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>()), Times.Once);
    }
    // Where repository returns null we should not throw error
    [Fact]
    public async void GetLogs_NullList()
    {
        // Arrange
        List<StrategyLogModel> logs = null;
        _mockStrategyLogRepository.Setup(x => x.GetLogs(It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>())).ReturnsAsync(logs.MapToStrategyLogDto());
        // Act
        var result = await _coinSpotExchangeService.GetLogs(DateTimeOffset.UtcNow, DateTimeOffset.UtcNow);
        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
        _mockStrategyLogRepository.Verify(x => x.GetLogs(It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>()), Times.Once);
    }
    #endregion
    #region GetLogsByStrategy
    // Test GetLogsByStrategy
    [Fact]
    public async void GetLogsByStrategy_Success()
    {
        // Arrange
        var logs = new List<StrategyLogModel>
        {
            new() { StrategyName = "Test", Message = "Test 1", Timestamp = DateTimeOffset.UtcNow },
            new() { StrategyName = "Test", Message = "Test 2", Timestamp = DateTimeOffset.UtcNow },
            new() { StrategyName = "Test", Message = "Test 3", Timestamp = DateTimeOffset.UtcNow }
        };
        _mockStrategyLogRepository.Setup(x => x.GetLogsByStrategy("Test", It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>())).ReturnsAsync(logs.MapToStrategyLogDto());
        // Act
        var result = await _coinSpotExchangeService.GetLogsByStrategy("Test", DateTimeOffset.UtcNow, DateTimeOffset.UtcNow);
        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Count);
        Assert.Equal("Test", result[0].StrategyName);
        Assert.Equal("Test 1", result[0].Message);
        Assert.Equal("Test", result[1].StrategyName);
        Assert.Equal("Test 2", result[1].Message);
        Assert.Equal("Test", result[2].StrategyName);
        Assert.Equal("Test 3", result[2].Message);
        _mockStrategyLogRepository.Verify(x => x.GetLogsByStrategy("Test", It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>()), Times.Once);
    }
    // Where repository throws exception we should throw error
    [Fact]
    public async void GetLogsByStrategy_RepositoryThrowsException()
    {
        // Arrange
        _mockStrategyLogRepository.Setup(x => x.GetLogsByStrategy("Test", It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>())).ReturnsAsync(() => throw new Exception());
        // Act
        await Assert.ThrowsAnyAsync<Exception>(() => _coinSpotExchangeService.GetLogsByStrategy("Test", DateTimeOffset.UtcNow, DateTimeOffset.UtcNow));
        // Assert
        _mockStrategyLogRepository.Verify(x => x.GetLogsByStrategy("Test", It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>()), Times.Once);
    }
    // Where repository returns empty list we should not throw error
    [Fact]
    public async void GetLogsByStrategy_EmptyList()
    {
        // Arrange
        var logs = new List<StrategyLogModel>();
        _mockStrategyLogRepository.Setup(x => x.GetLogsByStrategy("Test", It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>())).ReturnsAsync(logs.MapToStrategyLogDto());
        // Act
        var result = await _coinSpotExchangeService.GetLogsByStrategy("Test", DateTimeOffset.UtcNow, DateTimeOffset.UtcNow);
        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
        _mockStrategyLogRepository.Verify(x => x.GetLogsByStrategy("Test", It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>()), Times.Once);
    }
    // Where repository returns null we should not throw error
    [Fact]
    public async void GetLogsByStrategy_NullList()
    {
        // Arrange
        List<StrategyLogModel> logs = null;
        _mockStrategyLogRepository.Setup(x => x.GetLogsByStrategy("Test", It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>())).ReturnsAsync(logs.MapToStrategyLogDto());
        // Act
        var result = await _coinSpotExchangeService.GetLogsByStrategy("Test", DateTimeOffset.UtcNow, DateTimeOffset.UtcNow);
        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
        _mockStrategyLogRepository.Verify(x => x.GetLogsByStrategy("Test", It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>()), Times.Once);
    }
    #endregion
    #region SaveLog
    // Test SaveLog
    [Fact]
    public async void SaveLog_Success()
    {
        // Arrange
        var log = new StrategyLogModel(){StrategyName = "Test", Message = "Test", Timestamp = DateTimeOffset.UtcNow};
        _mockStrategyLogRepository.Setup(x => x.SaveLog(It.IsAny<StrategyLogDto>())).ReturnsAsync(true);
        // Act
        var result = await _coinSpotExchangeService.SaveLog(log);
        // Assert
        Assert.True(result);
        _mockStrategyLogRepository.Verify(x => x.SaveLog(It.IsAny<StrategyLogDto>()), Times.Once);
    }
    // Where repository throws exception we should throw error
    [Fact]
    public async void SaveLog_RepositoryThrowsException()
    {
        // Arrange
        var log = new StrategyLogModel(){StrategyName = "Test", Message = "Test", Timestamp = DateTimeOffset.UtcNow};
        _mockStrategyLogRepository.Setup(x => x.SaveLog(It.IsAny<StrategyLogDto>())).ReturnsAsync(() => throw new Exception());
        // Act
        await Assert.ThrowsAnyAsync<Exception>(() => _coinSpotExchangeService.SaveLog(log));
        // Assert
        _mockStrategyLogRepository.Verify(x => x.SaveLog(It.IsAny<StrategyLogDto>()), Times.Once);
    }
    // Where repository returns false we should not throw error
    [Fact]
    public async void SaveLog_Fails()
    {
        // Arrange
        var log = new StrategyLogModel(){StrategyName = "Test", Message = "Test", Timestamp = DateTimeOffset.UtcNow};
        _mockStrategyLogRepository.Setup(x => x.SaveLog(It.IsAny<StrategyLogDto>())).ReturnsAsync(false);
        // Act
        var result = await _coinSpotExchangeService.SaveLog(log);
        // Assert
        Assert.False(result);
        _mockStrategyLogRepository.Verify(x => x.SaveLog(It.IsAny<StrategyLogDto>()), Times.Once);
    }

    #endregion
    #region GetTargetWeightingsAsync
    // Test GetTargetWeightingsAsync
    [Fact]
    public async void GetTargetWeightingsAsync_Success()
    {
        // Arrange
        var positionTargetWeightings = new List<PositionTargetWeightingDto>
        {
            new(Exchange,Coin.BTC, 0.5m, DateTimeOffset.UtcNow.DateTime),
            new(Exchange,Coin.ETH, 0.3m, DateTimeOffset.UtcNow.DateTime)
        };
        _mockPositionTargetWeightingRepository.Setup(x => x.GetLatestWeightings()).ReturnsAsync(positionTargetWeightings);
        // Act
        var result = await _coinSpotExchangeService.GetPositionTargetWeightingsAsync();
        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Equal(Coin.BTC, result[0].Name);
        Assert.Equal(0.5m, result[0].TargetWeighting);
        Assert.Equal(Coin.ETH, result[1].Name);
        Assert.Equal(0.3m, result[1].TargetWeighting);
        _mockPositionTargetWeightingRepository.Verify(x => x.GetLatestWeightings(), Times.Once);
    }
    // Where repository throws exception we should throw error
    [Fact]
    public async void GetTargetWeightingsAsync_RepositoryThrowsException()
    {
        // Arrange
        _mockPositionTargetWeightingRepository.Setup(x => x.GetLatestWeightings()).ReturnsAsync(() => throw new Exception());
        // Act
        await Assert.ThrowsAnyAsync<Exception>(() => _coinSpotExchangeService.GetPositionTargetWeightingsAsync());
        // Assert
        _mockPositionTargetWeightingRepository.Verify(x => x.GetLatestWeightings(), Times.Once);
    }
    // Where repository returns empty list we should not throw error
    [Fact]
    public async void GetTargetWeightingsAsync_EmptyList()
    {
        // Arrange
        var positionTargetWeightings = new List<PositionTargetWeightingDto>();
        _mockPositionTargetWeightingRepository.Setup(x => x.GetLatestWeightings()).ReturnsAsync(positionTargetWeightings);
        // Act
        var result = await _coinSpotExchangeService.GetPositionTargetWeightingsAsync();
        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
        _mockPositionTargetWeightingRepository.Verify(x => x.GetLatestWeightings(), Times.Once);
    }
    // Where repository returns null we should not throw error
    [Fact]
    public async void GetTargetWeightingsAsync_NullList()
    {
        // Arrange
        List<PositionTargetWeightingDto> positionTargetWeightings = null;
        _mockPositionTargetWeightingRepository.Setup(x => x.GetLatestWeightings()).ReturnsAsync(positionTargetWeightings);
        // Act
        var result = await _coinSpotExchangeService.GetPositionTargetWeightingsAsync();
        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
        _mockPositionTargetWeightingRepository.Verify(x => x.GetLatestWeightings(), Times.Once);
    }
    #endregion
    #region SaveTargetWeightingAsync
    // Test SaveTargetWeightingAsync
    [Fact]
    public async void SaveTargetWeightingAsync_Success()
    {
        // Arrange
        List<PositionTargetWeightingModel> positionTargetWeightings = [new PositionTargetWeightingModel(now) { Name = Coin.BTC, TargetWeighting = 0.5m }];
        _mockPositionTargetWeightingRepository.Setup(x => x.SaveWeightings(It.IsAny<List<PositionTargetWeightingDto>>())).ReturnsAsync(true);
        // Act
        var result = await _coinSpotExchangeService.SavePositionTargetWeightingsAsync(positionTargetWeightings);
        // Assert
        Assert.True(result);
        _mockPositionTargetWeightingRepository.Verify(x => x.SaveWeightings(It.IsAny<List<PositionTargetWeightingDto>>()), Times.Once);
    }
    // Where repository throws exception we should throw error
    [Fact]
    public async void SaveTargetWeightingAsync_RepositoryThrowsException()
    {
        // Arrange
        List<PositionTargetWeightingModel> positionTargetWeightings = [new PositionTargetWeightingModel(now) { Name = Coin.BTC, TargetWeighting = 0.5m }];
        _mockPositionTargetWeightingRepository.Setup(x => x.SaveWeightings(It.IsAny<List<PositionTargetWeightingDto>>())).ReturnsAsync(() => throw new Exception());
        // Act
        await Assert.ThrowsAnyAsync<Exception>(() => _coinSpotExchangeService.SavePositionTargetWeightingsAsync(positionTargetWeightings));
        // Assert
        _mockPositionTargetWeightingRepository.Verify(x => x.SaveWeightings(It.IsAny<List<PositionTargetWeightingDto>>()), Times.Once);
    }
    // Where repository returns false we should not throw error
    [Fact]
    public async void SaveTargetWeightingAsync_Fails()
    {
        // Arrange
        List<PositionTargetWeightingModel> positionTargetWeightings = [new PositionTargetWeightingModel(now) { Name = Coin.BTC, TargetWeighting = 0.5m }];
        _mockPositionTargetWeightingRepository.Setup(x => x.SaveWeightings(It.IsAny<List<PositionTargetWeightingDto>>())).ReturnsAsync(false);
        // Act
        var result = await _coinSpotExchangeService.SavePositionTargetWeightingsAsync(positionTargetWeightings);
        // Assert
        Assert.False(result);
        _mockPositionTargetWeightingRepository.Verify(x => x.SaveWeightings(It.IsAny<List<PositionTargetWeightingDto>>()), Times.Once);
    }
    #endregion

    #region SaveDailyReturns

    // Test SaveDailyReturns
    [Fact]
    public async void SaveDailyReturns_Success()
    {
        // Arrange
        var previousDayReturns = new Dictionary<string, decimal>
        {
            {Coin.BTC, 0.5m},
            {Coin.ETH, 0.3m}
        };
        _mockReturnRepository.Setup(x => x.SaveReturns(It.IsAny<List<ReturnDto>>())).ReturnsAsync(true);
        // Act
        var result = await _coinSpotExchangeService.SaveDailyReturns(previousDayReturns);
        // Assert
        Assert.True(result);
        _mockReturnRepository.Verify(x => x.SaveReturns(It.IsAny<List<ReturnDto>>()), Times.Once);
    }
    // Where repository throws exception we should throw error
    [Fact]
    public async void SaveDailyReturns_RepositoryThrowsException()
    {
        // Arrange
        var previousDayReturns = new Dictionary<string, decimal>
        {
            {Coin.BTC, 0.5m},
            {Coin.ETH, 0.3m}
        };
        _mockReturnRepository.Setup(x => x.SaveReturns(It.IsAny<List<ReturnDto>>())).ReturnsAsync(() => throw new Exception());
        // Act
        await Assert.ThrowsAnyAsync<Exception>(() => _coinSpotExchangeService.SaveDailyReturns(previousDayReturns));
        // Assert
        _mockReturnRepository.Verify(x => x.SaveReturns(It.IsAny<List<ReturnDto>>()), Times.Once);
    }
    // Where repository returns false we should not throw error
    [Fact]
    public async void SaveDailyReturns_Fails()
    {
        // Arrange
        var previousDayReturns = new Dictionary<string, decimal>
        {
            {Coin.BTC, 0.5m},
            {Coin.ETH, 0.3m}
        };
        _mockReturnRepository.Setup(x => x.SaveReturns(It.IsAny<List<ReturnDto>>())).ReturnsAsync(false);
        // Act
        var result = await _coinSpotExchangeService.SaveDailyReturns(previousDayReturns);
        // Assert
        Assert.False(result);
        _mockReturnRepository.Verify(x => x.SaveReturns(It.IsAny<List<ReturnDto>>()), Times.Once);
    }

    #endregion
}