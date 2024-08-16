using Microsoft.Extensions.DependencyInjection;
using Moq;
using TradingBot.Domain.Enum;
using TradingBot.Domain.Executor;
using TradingBot.Domain.Model;
using TradingBot.Domain.Strategy;
using TradingBot.Domain.TimeProvider;
using TradingBot.Usecases.Executor;
using TradingBot.Usecases.Strategy;

namespace TradingBot.Usecases.Tests.Executor;

public class StrategyExecutorTests
{
    private readonly TimeProvider _timeProvider = new StaticTimeProvider(DateTimeOffset.UtcNow);
    private readonly Mock<IStrategyFactory> _mockStrategyFactory = new();
    private readonly StrategyExecutor _executor = new();

    // Test for GenerateSignals
    [Fact]
    public async Task GenerateSignals_WhenCalled_ReturnsTradeSignalModelList()
    {
        // Arrange
        var strategyConfig = new StrategyExecutorConfig()
        {
            StrategyConfigs = new List<StrategyConfig>()
            {
                new StrategyConfig()
                {
                    StrategyName = FixedAllocationStrategy.Name,
                    Weight = 1,
                    Config = new Dictionary<string, string>()
                    {
                        {"AUD", "0.5"},
                        {Coin.BTC, "0.5"}
                    }
                }
            }
        };
        var mockExecutorStrategy = new Mock<IStrategy>();
        mockExecutorStrategy.Setup(x => x.GenerateSignals(It.IsAny<StrategyConfig>()))
            .ReturnsAsync(new List<TradeSignalModel>()
            {
                new TradeSignalModel()
                {
                    TickerName = Coin.BTC,
                    Quantity = 0.5m,
                    Action = TradeAction.Buy,
                    StrategyName = FixedAllocationStrategy.Name,
                    Timestamp = _timeProvider.GetUtcNow()
                }
            });
        _mockStrategyFactory.Setup(x => x.GetStrategy(It.IsAny<string>()))
            .Returns(mockExecutorStrategy.Object);

        // Act
        var result = await _executor.GenerateSignals(_mockStrategyFactory.Object, _timeProvider, strategyConfig);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal(Coin.BTC, result[0].TickerName);
        Assert.Equal(0.5m, result[0].Quantity);
        Assert.Equal(_timeProvider.GetUtcNow(), result[0].Timestamp);
        Assert.Equal(FixedAllocationStrategy.Name, result[0].StrategyName);
        Assert.Equal(TradeAction.Buy, result[0].Action);
    }
    // Test for GenerateSignals when strategyConfig is null
    [Fact]
    public async Task GenerateSignals_WhenStrategyConfigIsNull_ReturnsEmptyTradeSignalModelList()
    {
        // Arrange
        var strategyConfig = new StrategyExecutorConfig()
        {
            StrategyConfigs = null
        };

        // Act
        var result = await _executor.GenerateSignals(_mockStrategyFactory.Object, _timeProvider, strategyConfig);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }
    // Test for GenerateSignals when strategyConfig weight is more than 1
    [Fact]
    public async Task GenerateSignals_WhenStrategyConfigWeightIsMoreThan1_ThrowsInvalidOperationException()
    {
        // Arrange
        var strategyConfig = new StrategyExecutorConfig()
        {
            StrategyConfigs = new List<StrategyConfig>()
            {
                new StrategyConfig()
                {
                    StrategyName = FixedAllocationStrategy.Name,
                    Weight = 1.5m,
                    Config = new Dictionary<string, string>()
                    {
                        {"AUD", "0.5"},
                        {Coin.BTC, "0.5"}
                    }
                }
            }
        };

        // Act and Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _executor.GenerateSignals(_mockStrategyFactory.Object, _timeProvider, strategyConfig));
    }
    // Test for GenerateSignals when strategyConfig contains multiple different strategies
    [Fact]
    public async Task GenerateSignals_WhenStrategyConfigContainsMultipleStrategies_ReturnsTradeSignalModelList()
    {
        // Arrange
        var strategyConfig = new StrategyExecutorConfig()
        {
            StrategyConfigs = new List<StrategyConfig>()
            {
                new StrategyConfig()
                {
                    StrategyName = FixedAllocationStrategy.Name,
                    Weight = 0.5m,
                    Config = new Dictionary<string, string>()
                    {
                        {"AUD", "0.5"},
                        {Coin.BTC, "0.5"}
                    }
                },
                new StrategyConfig()
                {
                    StrategyName = FixedAllocationStrategy.Name,
                    Weight = 0.5m,
                    Config = new Dictionary<string, string>()
                    {
                        {"AUD", "0.5"},
                        {Coin.ETH, "0.5"}
                    }
                }
            }
        };
        var mockExecutorStrategy = new Mock<IStrategy>();
        // mockExecutorStrategy.GenerateSignals returns a different list of TradeSignalModel for each StrategyConfig
        mockExecutorStrategy.SetupSequence(x => x.GenerateSignals(It.IsAny<StrategyConfig>()))
            .ReturnsAsync(new List<TradeSignalModel>()
            {
                new TradeSignalModel()
                {
                    TickerName = Coin.BTC,
                    Quantity = 0.5m,
                    Action = TradeAction.Buy,
                    StrategyName = FixedAllocationStrategy.Name,
                    Timestamp = _timeProvider.GetUtcNow()
                }
            })
            .ReturnsAsync(new List<TradeSignalModel>()
            {
                new TradeSignalModel()
                {
                    TickerName = Coin.ETH,
                    Quantity = 0.5m,
                    Action = TradeAction.Buy,
                    StrategyName = FixedAllocationStrategy.Name,
                    Timestamp = _timeProvider.GetUtcNow()
                }
            });
        _mockStrategyFactory.Setup(x => x.GetStrategy(It.IsAny<string>()))
            .Returns(mockExecutorStrategy.Object);

        // Act
        var result = await _executor.GenerateSignals(_mockStrategyFactory.Object, _timeProvider, strategyConfig);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Equal(Coin.BTC, result[0].TickerName);
        Assert.Equal(0.25m, result[0].Quantity);
        Assert.Equal(_timeProvider.GetUtcNow(), result[0].Timestamp);
        Assert.Equal(FixedAllocationStrategy.Name, result[0].StrategyName);
        Assert.Equal(TradeAction.Buy, result[0].Action);
        Assert.Equal(Coin.ETH, result[1].TickerName);
        Assert.Equal(0.25m, result[1].Quantity);
        Assert.Equal(_timeProvider.GetUtcNow(), result[1].Timestamp);
        Assert.Equal(FixedAllocationStrategy.Name, result[1].StrategyName);
        Assert.Equal(TradeAction.Buy, result[1].Action);
    }
    // Test for GenerateSignals when strategyConfig contains multiple strategies with different weights
    [Fact]
    public async Task
        GenerateSignals_WhenStrategyConfigContainsMultipleStrategiesWithDifferentWeights_ReturnsTradeSignalModelList()
    {
        // Arrange
        var strategyConfig = new StrategyExecutorConfig()
        {
            StrategyConfigs =
            [
                new StrategyConfig()
                {
                    StrategyName = FixedAllocationStrategy.Name,
                    Weight = 0.25m,
                    Config = new Dictionary<string, string>()
                    {
                        { "AUD", "0.5" },
                        { Coin.BTC, "0.5" }
                    }
                },

                new StrategyConfig()
                {
                    StrategyName = FixedAllocationStrategy.Name,
                    Weight = 0.75m,
                    Config = new Dictionary<string, string>()
                    {
                        { "AUD", "0.5" },
                        { Coin.ETH, "0.5" }
                    }
                }
            ]
        };
        var mockExecutorStrategy = new Mock<IStrategy>();
        // mockExecutorStrategy.GenerateSignals returns a different list of TradeSignalModel for each StrategyConfig
        mockExecutorStrategy.SetupSequence(x => x.GenerateSignals(It.IsAny<StrategyConfig>()))
            .ReturnsAsync([
                new TradeSignalModel()
                {
                    TickerName = Coin.BTC,
                    Quantity = 0.5m,
                    Action = TradeAction.Buy,
                    StrategyName = FixedAllocationStrategy.Name,
                    Timestamp = _timeProvider.GetUtcNow()
                }
            ])
            .ReturnsAsync([
                new TradeSignalModel()
                {
                    TickerName = Coin.ETH,
                    Quantity = 0.5m,
                    Action = TradeAction.Buy,
                    StrategyName = FixedAllocationStrategy.Name,
                    Timestamp = _timeProvider.GetUtcNow()
                }
            ]);
        _mockStrategyFactory.Setup(x => x.GetStrategy(It.IsAny<string>()))
            .Returns(mockExecutorStrategy.Object);

        // Act
        var result = await _executor.GenerateSignals(_mockStrategyFactory.Object, _timeProvider, strategyConfig);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        
        Assert.Equal(Coin.BTC, result[0].TickerName);
        Assert.Equal(0.125m, result[0].Quantity);
        Assert.Equal(_timeProvider.GetUtcNow(), result[0].Timestamp);
        Assert.Equal(FixedAllocationStrategy.Name, result[0].StrategyName);

        Assert.Equal(Coin.ETH, result[1].TickerName);
        Assert.Equal(0.375m, result[1].Quantity);
        Assert.Equal(_timeProvider.GetUtcNow(), result[1].Timestamp);
        Assert.Equal(FixedAllocationStrategy.Name, result[1].StrategyName);
    }
    
    // test GGenerateSignals when strategyConfig contains multiple strategies with equal weights and competing buy vs sell signals
    [Fact]
    public async Task
        GenerateSignals_WhenStrategyConfigContainsMultipleStrategiesWithEqualWeightsAndCompetingBuyVsSellSignals_ReturnsTradeSignalModelList()
    {
        // Arrange
        var strategyConfig = new StrategyExecutorConfig()
        {
            StrategyConfigs =
            [
                new StrategyConfig()
                {
                    StrategyName = FixedAllocationStrategy.Name,
                    Weight = 0.5m,
                    Config = new Dictionary<string, string>()
                    {
                        { "AUD", "0.5" },
                        { Coin.BTC, "0.5" }
                    }
                },

                new StrategyConfig()
                {
                    StrategyName = FixedAllocationStrategy.Name,
                    Weight = 0.5m,
                    Config = new Dictionary<string, string>()
                    {
                        { "AUD", "0.5" },
                        { Coin.ETH, "0.5" }
                    }
                }
            ]
        };
        var mockExecutorStrategy = new Mock<IStrategy>();
        // mockExecutorStrategy.GenerateSignals returns a different list of TradeSignalModel for each StrategyConfig
        mockExecutorStrategy.SetupSequence(x => x.GenerateSignals(It.IsAny<StrategyConfig>()))
            .ReturnsAsync([
                new TradeSignalModel()
                {
                    TickerName = Coin.ETH,
                    Quantity = 0.6m,
                    Action = TradeAction.Buy,
                    StrategyName = FixedAllocationStrategy.Name,
                    Timestamp = _timeProvider.GetUtcNow()
                }
            ])
            .ReturnsAsync([
                new TradeSignalModel()
                {
                    TickerName = Coin.ETH,
                    Quantity = -0.5m,
                    Action = TradeAction.Sell,
                    StrategyName = FixedAllocationStrategy.Name,
                    Timestamp = _timeProvider.GetUtcNow()
                }
            ]);
        _mockStrategyFactory.Setup(x => x.GetStrategy(It.IsAny<string>()))
            .Returns(mockExecutorStrategy.Object);

        // Act
        var result = await _executor.GenerateSignals(_mockStrategyFactory.Object, _timeProvider, strategyConfig);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Count);
        
        Assert.Equal(Coin.ETH, result[0].TickerName);
        Assert.Equal(0.05m, result[0].Quantity);
        Assert.Equal(_timeProvider.GetUtcNow(), result[0].Timestamp);
        Assert.Equal($"{FixedAllocationStrategy.Name};{FixedAllocationStrategy.Name}", result[0].StrategyName);
        Assert.Equal(TradeAction.Buy, result[0].Action);
    }
}