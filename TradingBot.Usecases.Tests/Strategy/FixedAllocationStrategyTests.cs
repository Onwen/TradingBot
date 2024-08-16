using Moq;
using TradingBot.Domain.Enum;
using TradingBot.Domain.Executor;
using TradingBot.Domain.Model;
using TradingBot.Domain.Service;
using TradingBot.Domain.TimeProvider;
using TradingBot.Usecases.Strategy;

namespace TradingBot.Usecases.Tests.Strategy;

public class FixedAllocationStrategyTests
{
    private readonly Mock<IPortfolioService> _mockPortfolioService; 
    private readonly Mock<IPricingService> _mockPricingService; 
    private readonly TimeProvider _timeProvider;
    private FixedAllocationStrategy strategy;
    public FixedAllocationStrategyTests()
    {
        _mockPortfolioService = new Mock<IPortfolioService>();
        _mockPricingService = new Mock<IPricingService>();
        _timeProvider = new StaticTimeProvider(DateTimeOffset.UtcNow);
        strategy = new FixedAllocationStrategy(_mockPortfolioService.Object, _mockPricingService.Object, _timeProvider);
    }
    
    // test strategy for generating signals
    [Fact]
    public async Task GenerateSignals_WhenCalled_ReturnsTradeSignals()
    {
        // Arrange
        var config = new StrategyConfig
        {
            Config = new Dictionary<string, string>
            {
                { Coin.BTC, "0.5" },
                { Coin.ETH, "0.5" }
            }
        };
        var portfolio = new PortfolioModel
        {
            Positions = new List<PositionModel>
            {
                new PositionModel(_timeProvider.GetUtcNow()) { Name = Coin.BTC, Quantity = 0.5m, CurrentPrice = 50000},
                new PositionModel(_timeProvider.GetUtcNow()) { Name = Coin.ETH, Quantity = 0.5m, CurrentPrice = 2000 }
            }
        };
        var assetPrices = new List<PriceSnapshotModel>
        {
            new PriceSnapshotModel { Name = Coin.BTC, Last = 50000 },
            new PriceSnapshotModel { Name = Coin.ETH, Last = 2000 }
        };
        _mockPortfolioService.Setup(x => x.GetPortfoliosAsync()).ReturnsAsync(portfolio);
        _mockPricingService.Setup(x => x.GetPriceSnapshotsAsync()).ReturnsAsync(assetPrices);
        
        // Act
        var signals = await strategy.GenerateSignals(config);
        
        // Assert
        Assert.Equal(2, signals.Count);
        Assert.Equal(Coin.BTC, signals[0].TickerName);
        Assert.Equal(-0.24m, signals[0].Quantity);
        Assert.Equal(TradeAction.Sell, signals[0].Action);
        Assert.Equal(FixedAllocationStrategy.Name, signals[0].StrategyName);
        Assert.Equal(Coin.ETH, signals[1].TickerName);
        Assert.Equal(6, signals[1].Quantity);
        Assert.Equal(TradeAction.Buy, signals[1].Action);
        Assert.Equal(FixedAllocationStrategy.Name, signals[1].StrategyName);
    }
    
    // test strategy when current portfolio has BTC and ETH but target allocation is for LTC and XRP
    [Fact]
    public async Task GenerateSignals_WhenCalledWithDifferentTargetAllocations_ReturnsTradeSignals()
    {
        // Arrange
        var config = new StrategyConfig
        {
            Config = new Dictionary<string, string>
            {
                { Coin.LTC, "0.5" },
                { Coin.XRP, "0.5" }
            }
        };
        var portfolio = new PortfolioModel
        {
            Positions = new List<PositionModel>
            {
                new PositionModel(_timeProvider.GetUtcNow()) { Name = Coin.BTC, Quantity = 0.5m, CurrentPrice = 50000},
                new PositionModel(_timeProvider.GetUtcNow()) { Name = Coin.ETH, Quantity = 0.5m, CurrentPrice = 2000 }
            }
        };
        var assetPrices = new List<PriceSnapshotModel>
        {
            new PriceSnapshotModel { Name = Coin.BTC, Last = 50000 },
            new PriceSnapshotModel { Name = Coin.ETH, Last = 2000 },
            new PriceSnapshotModel { Name = Coin.LTC, Last = 200 },
            new PriceSnapshotModel { Name = Coin.XRP, Last = 1 }
        };
        _mockPortfolioService.Setup(x => x.GetPortfoliosAsync()).ReturnsAsync(portfolio);
        _mockPricingService.Setup(x => x.GetPriceSnapshotsAsync()).ReturnsAsync(assetPrices);
        
        // Act
        var signals = await strategy.GenerateSignals(config);
        
        // Assert
        Assert.Equal(4, signals.Count);
        // assert it sells btc and eth to buy ltc and xrp
        Assert.Equal(Coin.LTC, signals[0].TickerName);
        Assert.Equal(65, signals[0].Quantity);
        Assert.Equal(TradeAction.Buy, signals[0].Action);
        Assert.Equal(FixedAllocationStrategy.Name, signals[0].StrategyName);
        Assert.Equal(Coin.XRP, signals[1].TickerName);
        Assert.Equal(13000, signals[1].Quantity);
        Assert.Equal(TradeAction.Buy, signals[1].Action);
        Assert.Equal(FixedAllocationStrategy.Name, signals[1].StrategyName);
        Assert.Equal(Coin.BTC, signals[2].TickerName);
        Assert.Equal(-0.5m, signals[2].Quantity);
        Assert.Equal(TradeAction.Sell, signals[2].Action);
        Assert.Equal(FixedAllocationStrategy.Name, signals[2].StrategyName);
        Assert.Equal(Coin.ETH, signals[3].TickerName);
        Assert.Equal(-0.5m, signals[3].Quantity);
        Assert.Equal(TradeAction.Sell, signals[3].Action);
        Assert.Equal(FixedAllocationStrategy.Name, signals[3].StrategyName);
    }
    
    // test strategy when target allocation is for BTC and ETH but current portfolio has only BTC
    [Fact]
    public async Task GenerateSignals_WhenCalledWithMissingAssetInPortfolio_ReturnsTradeSignals()
    {
        // Arrange
        var config = new StrategyConfig
        {
            Config = new Dictionary<string, string>
            {
                { Coin.BTC, "0.5" },
                { Coin.ETH, "0.5" }
            }
        };
        var portfolio = new PortfolioModel
        {
            Positions =
            [
                new PositionModel(_timeProvider.GetUtcNow()) { Name = Coin.BTC, Quantity = 0.5m, CurrentPrice = 50000 }
            ]
        };
        var assetPrices = new List<PriceSnapshotModel>
        {
            new PriceSnapshotModel { Name = Coin.BTC, Last = 50000 },
            new PriceSnapshotModel { Name = Coin.ETH, Last = 2000 }
        };
        _mockPortfolioService.Setup(x => x.GetPortfoliosAsync()).ReturnsAsync(portfolio);
        _mockPricingService.Setup(x => x.GetPriceSnapshotsAsync()).ReturnsAsync(assetPrices);
        
        // Act
        var signals = await strategy.GenerateSignals(config);
        
        // Assert
        Assert.Equal(2, signals.Count);
        Assert.Equal(Coin.BTC, signals[0].TickerName);
        Assert.Equal(-0.25m, signals[0].Quantity);
        Assert.Equal(TradeAction.Sell, signals[0].Action);
        Assert.Equal(FixedAllocationStrategy.Name, signals[0].StrategyName);
        Assert.Equal(Coin.ETH, signals[1].TickerName);
        Assert.Equal(6.25m, signals[1].Quantity);
        Assert.Equal(TradeAction.Buy, signals[1].Action);
        Assert.Equal(FixedAllocationStrategy.Name, signals[1].StrategyName);
    }
}