using Microsoft.Extensions.Logging;
using Moq;
using TradingBot.Domain.Model;
using TradingBot.Domain.Service;
using TradingBot.Domain.TimeProvider;
using TradingBot.Usecases.Strategy;

namespace TradingBot.Usecases.Tests.Strategy;

public class GetPriceSnapshotsStrategyTests
{
    private readonly Mock<IExchangeService> _mockExchangeService;
    private readonly TimeProvider _timeProvider;
    private readonly Mock<ILogger<GetPriceSnapshotsStrategy>> _mockLogger;
    private readonly GetPriceSnapshotsStrategy _getPriceSnapshotsStrategy;
    private readonly DateTimeOffset now = DateTimeOffset.UtcNow;
    
    public GetPriceSnapshotsStrategyTests()
    {
        _mockExchangeService = new Mock<IExchangeService>();
        _timeProvider = new StaticTimeProvider(now);
        _mockLogger = new Mock<ILogger<GetPriceSnapshotsStrategy>>();
        _getPriceSnapshotsStrategy = new GetPriceSnapshotsStrategy(_mockExchangeService.Object, _timeProvider, _mockLogger.Object);
    }
    // test should execute always returns true
    [Fact]
    public async Task ShouldExecute_Always_ReturnsTrue()
    {
        // Act
        var result = await _getPriceSnapshotsStrategy.ShouldExecute();
        
        // Assert
        Assert.True(result);
    }
    // test sleep time returns correct time
    [Fact]
    public void SleepTime_ReturnsCorrectTime()
    {
        // Arrange
        var nextFiveMinute = new DateTimeOffset(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0, now.Offset);
        var remainder = nextFiveMinute.Minute % 5;
        nextFiveMinute = nextFiveMinute.AddMinutes(5 - remainder);
        var timeToSleep = nextFiveMinute - now;
        
        // Act
        var result = _getPriceSnapshotsStrategy.SleepTime();
        
        // Assert
        Assert.Equal((int)timeToSleep.TotalMilliseconds, result);
    }
    // test handle execute calls GetPriceSnapshotsAsync and SaveLog
    [Fact]
    public async Task HandleExecute_CallsGetPriceSnapshotsAsyncAndSaveLog()
    {
        // Act
        await _getPriceSnapshotsStrategy.HandleExecute();
        
        // Assert
        _mockExchangeService.Verify(x => x.GetPriceSnapshotsAsync(), Times.Once);
        _mockExchangeService.Verify(x => x.SaveLog(It.IsAny<StrategyLogModel>()), Times.Once);
    }
}