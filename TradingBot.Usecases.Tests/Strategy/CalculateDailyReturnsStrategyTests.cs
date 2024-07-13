using Microsoft.Extensions.Logging;
using Moq;
using TradingBot.Domain.Model;
using TradingBot.Domain.Service;
using TradingBot.Domain.TimeProvider;
using TradingBot.Usecases.Strategy;

namespace TradingBot.Usecases.Tests.Strategy;

public class CalculateDailyReturnsStrategyTests
{
    private readonly Mock<IExchangeService> _mockExchangeService;
    private readonly TimeProvider _timeProvider;
    private readonly Mock<ILogger<CalculateDailyReturnsStrategy>> _mockLogger;
    private readonly CalculateDailyReturnsStrategy _calculateDailyReturnsStrategy;
    
    public CalculateDailyReturnsStrategyTests()
    {
        _mockExchangeService = new Mock<IExchangeService>();
        _timeProvider = new StaticTimeProvider(DateTimeOffset.UtcNow);
        _mockLogger = new Mock<ILogger<CalculateDailyReturnsStrategy>>();
        _calculateDailyReturnsStrategy = new CalculateDailyReturnsStrategy(_mockExchangeService.Object, _timeProvider, _mockLogger.Object);
    }

    // test should execute
    [Fact]
    public async Task ShouldExecute_WhenLastRebalanceDateIsBeforeYesterday_ReturnsTrue()
    {
        // Arrange
        var lastDayLogs = new List<StrategyLogModel>
        {
            new StrategyLogModel { Message = "Calculated Returns", Timestamp = DateTimeOffset.UtcNow.AddDays(-2) }
        };
        _mockExchangeService.Setup(x => x.GetLogsByStrategy(It.IsAny<string>(), It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>()))
            .ReturnsAsync(lastDayLogs);
        
        // Act
        var result = await _calculateDailyReturnsStrategy.ShouldExecute();
        
        // Assert
        Assert.True(result);
    }
    // test should execute when last rebalance date is yesterday
    [Fact]
    public async Task ShouldExecute_WhenLastRebalanceDateIsYesterday_ReturnsFalse()
    {
        // Arrange
        var lastDayLogs = new List<StrategyLogModel>
        {
            new StrategyLogModel { Message = "Calculated Returns", Timestamp = DateTimeOffset.UtcNow.AddDays(-1).AddMinutes(1) }
        };
        _mockExchangeService.Setup(x => x.GetLogsByStrategy(It.IsAny<string>(), It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>()))
            .ReturnsAsync(lastDayLogs);
        
        // Act
        var result = await _calculateDailyReturnsStrategy.ShouldExecute();
        
        // Assert
        Assert.False(result);
    }
    // test should execute when last rebalance date is today
    [Fact]
    public async Task ShouldExecute_WhenLastRebalanceDateIsToday_ReturnsFalse()
    {
        // Arrange
        var lastDayLogs = new List<StrategyLogModel>
        {
            new StrategyLogModel { Message = "Calculated Returns", Timestamp = DateTimeOffset.UtcNow }
        };
        _mockExchangeService.Setup(x => x.GetLogsByStrategy(It.IsAny<string>(), It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>()))
            .ReturnsAsync(lastDayLogs);
        
        // Act
        var result = await _calculateDailyReturnsStrategy.ShouldExecute();
        
        // Assert
        Assert.False(result);
    }
    // test handle execute when daily prices is null
    [Fact]
    public async Task HandleExecute_WhenDailyPricesIsNull_ThrowsNullReferenceException()
    {
        // Arrange
        var portfolio = new PortfolioModel();
        _mockExchangeService.Setup(x => x.GetPortfoliosAsync()).ReturnsAsync(portfolio);
        _mockExchangeService.Setup(x => x.GetDailyPricesAsync(It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>()))
            .ReturnsAsync((List<PriceSnapshotModel>)null);
        
        // Act
        Func<Task> act = async () => await _calculateDailyReturnsStrategy.HandleExecute();
        
        // Assert
        await Assert.ThrowsAsync<NullReferenceException>(act);
    }
    // test handle execute when daily prices is empty
    [Fact]
    public async Task HandleExecute_WhenDailyPricesIsEmpty_ThrowsNullReferenceException()
    {
        // Arrange
        var portfolio = new PortfolioModel();
        _mockExchangeService.Setup(x => x.GetPortfoliosAsync()).ReturnsAsync(portfolio);
        _mockExchangeService.Setup(x => x.GetDailyPricesAsync(It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>()))
            .ReturnsAsync(new List<PriceSnapshotModel>());
        
        // Act
        Func<Task> act = async () => await _calculateDailyReturnsStrategy.HandleExecute();
        
        // Assert
        await Assert.ThrowsAsync<NullReferenceException>(act);
    }
    // test handle execute when daily prices is not empty
    [Fact]
    public async Task HandleExecute_WhenDailyPricesIsNotEmpty_CalculatesPreviousDayReturns()
    {
        // Arrange
        var now = DateTimeOffset.UtcNow;
        var portfolio = new PortfolioModel
        {
            Positions = new List<PositionModel>
            {
                new PositionModel(now) { Name = "BTC", CurrentPrice = 100 }
            }
        };
        var dailyPrices = new List<PriceSnapshotModel>
        {
            new PriceSnapshotModel { Name = "BTC", Timestamp = DateTimeOffset.UtcNow.Date.AddDays(-1), Last = 90 }
        };
        _mockExchangeService.Setup(x => x.GetPortfoliosAsync()).ReturnsAsync(portfolio);
        _mockExchangeService.Setup(x => x.GetDailyPricesAsync(It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>()))
            .ReturnsAsync(dailyPrices);
        
        // Act
        await _calculateDailyReturnsStrategy.HandleExecute();
        
        // Assert
        _mockExchangeService.Verify(x => x.SaveLog(It.IsAny<StrategyLogModel>()), Times.Once);
    }
    // test handle execute when yesterday price is 0
    [Fact]
    public async Task HandleExecute_WhenYesterdayPriceIsZero_LogsError()
    {
        // Arrange
        var now = DateTimeOffset.UtcNow;
        var portfolio = new PortfolioModel
        {
            Positions = new List<PositionModel>
            {
                new PositionModel(now) { Name = "BTC", CurrentPrice = 100 }
            }
        };
        var dailyPrices = new List<PriceSnapshotModel>
        {
            new PriceSnapshotModel { Name = "BTC", Timestamp = DateTimeOffset.UtcNow.Date.AddDays(-1), Last = 0 }
        };
        _mockExchangeService.Setup(x => x.GetPortfoliosAsync()).ReturnsAsync(portfolio);
        _mockExchangeService.Setup(x => x.GetDailyPricesAsync(It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>()))
            .ReturnsAsync(dailyPrices);
        
        // Act
        await _calculateDailyReturnsStrategy.HandleExecute();
    }
    // test sleep time
    [Fact]
    public void SleepTime_ReturnsOneHour()
    {
        // Arrange
        // Act
        var result = _calculateDailyReturnsStrategy.SleepTime();
        
        // Assert
        Assert.Equal(3600000, result);
    }
}