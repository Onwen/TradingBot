using Moq;
using TradingBot.Domain.Enum;
using TradingBot.Domain.Model;
using TradingBot.Domain.Service;
using TradingBot.Domain.TimeProvider;
using TradingBot.Usecases.Executor;

namespace TradingBot.Usecases.Tests.Executor;

public class TradeExecutorTests
{
    private readonly Mock<IPortfolioService> _mockPortfolioService;
    private readonly Mock<IPricingService> _mockPricingService;
    private readonly Mock<IExchangeService> _exchangeServiceMock;
    private readonly StaticTimeProvider _timeProvider;
    private readonly TradeExecutor _tradeExecutor;
    public TradeExecutorTests()
    {
        _mockPortfolioService = new Mock<IPortfolioService>();
        _mockPricingService = new Mock<IPricingService>();
        _exchangeServiceMock = new Mock<IExchangeService>();
        _timeProvider = new StaticTimeProvider(DateTimeOffset.UtcNow);
        _tradeExecutor = new TradeExecutor();
    }
    
    // test that the trade executor can execute trades
    [Fact]
    public async Task ExecuteTrades_WhenCalled_ReturnsMarketOrderModels()
    {
        // Arrange
        var tradeSignals = new List<TradeSignalModel>
        {
            new()
            {
                Action = TradeAction.Sell,
                Quantity = -1,
                TickerName = Coin.BTC
            },
            new()
            {
                Action = TradeAction.Buy,
                Quantity = 1,
                TickerName = Coin.ETH
            }
        };
        var portfolioPreSale = new PortfolioModel
        {
            Positions = new List<PositionModel>
            {
                new(_timeProvider.GetUtcNow())
                {
                    Name = Coin.BTC,
                    Quantity = 1
                }
            }
        };
        var portfolioPostSale = new PortfolioModel
        {
            Positions = new List<PositionModel>
            {
                new(_timeProvider.GetUtcNow())
                {
                    Name = "AUD",
                    Quantity = 1000
                }
            }
        };
        var priceSnapshots = new List<PriceSnapshotModel>
        {
            new()
            {
                Name = Coin.BTC,
                Bid = 1000
            },
            new()
            {
                Name = Coin.ETH,
                Bid = 100
            }
        };
        var responseQueue = new Queue<PortfolioModel>();
        responseQueue.Enqueue(portfolioPreSale);
        responseQueue.Enqueue(portfolioPostSale);

        _mockPortfolioService.Setup(m => m.GetPortfoliosAsync())
            .Returns(() => Task.FromResult(responseQueue.Dequeue()));
        _mockPricingService.Setup(b => b.GetPriceSnapshotsAsync()).ReturnsAsync(priceSnapshots);
        _exchangeServiceMock.Setup(b => b.MarketSellAsync(Coin.BTC, 1)).ReturnsAsync(new MarketOrderModel
        {
            Coin = Coin.BTC,
            Amount = 1,
            OrderType = "Sell",
            Rate = 1000
        });
        _exchangeServiceMock.Setup(b => b.MarketBuyAsync(Coin.ETH, 1)).ReturnsAsync(new MarketOrderModel
        {
            Coin = Coin.ETH,
            Amount = 1,
            OrderType = "Buy",
            Rate = 100
        });

        // Act
        var result = await _tradeExecutor.ExecuteTrades(_mockPortfolioService.Object, _mockPricingService.Object, _exchangeServiceMock.Object, tradeSignals);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal(Coin.BTC, result[0].Coin);
        Assert.Equal(1, result[0].Amount);
        Assert.Equal("Sell", result[0].OrderType);
        Assert.Equal(1000, result[0].Rate);
        Assert.Equal(Coin.ETH, result[1].Coin);
        Assert.Equal(1, result[1].Amount);
        Assert.Equal("Buy", result[1].OrderType);
        Assert.Equal(100, result[1].Rate);
    }
    
    // test that the trade executor can handle no trades
    [Fact]
    public async Task ExecuteTrades_WhenNoTrades_ReturnsEmptyList()
    {
        // Arrange
        var tradeSignals = new List<TradeSignalModel>();
        var portfolio = new PortfolioModel
        {
            Positions = new List<PositionModel>
            {
                new(_timeProvider.GetUtcNow())
                {
                    Name = "AUD",
                    Quantity = 1000
                }
            }
        };
        _mockPortfolioService.Setup(m => m.GetPortfoliosAsync()).ReturnsAsync(portfolio);
        
        // Act
        var result = await _tradeExecutor.ExecuteTrades(_mockPortfolioService.Object, _mockPricingService.Object, _exchangeServiceMock.Object, tradeSignals);

        // Assert
        Assert.Empty(result);
    }
    
    // test that the trade executor can handle no sales
    [Fact]
    public async Task ExecuteTrades_WhenNoSales_ReturnsBuyMarketOrderModels()
    {
        // Arrange
        var tradeSignals = new List<TradeSignalModel>
        {
            new()
            {
                Action = TradeAction.Buy,
                Quantity = 1,
                TickerName = Coin.ETH
            }
        };
        var portfolio = new PortfolioModel
        {
            Positions = new List<PositionModel>
            {
                new(_timeProvider.GetUtcNow())
                {
                    Name = "AUD",
                    Quantity = 1000
                }
            }
        };
        var priceSnapshots = new List<PriceSnapshotModel>
        {
            new()
            {
                Name = Coin.ETH,
                Bid = 100
            }
        };

        _mockPortfolioService.Setup(m => m.GetPortfoliosAsync()).ReturnsAsync(portfolio);
        _mockPricingService.Setup(b => b.GetPriceSnapshotsAsync()).ReturnsAsync(priceSnapshots);
        _exchangeServiceMock.Setup(b => b.MarketBuyAsync(Coin.ETH, 1)).ReturnsAsync(new MarketOrderModel
        {
            Coin = Coin.ETH,
            Amount = 1,
            OrderType = "Buy",
            Rate = 100
        });

        // Act
        var result = await _tradeExecutor.ExecuteTrades(_mockPortfolioService.Object, _mockPricingService.Object, _exchangeServiceMock.Object, tradeSignals);

        // Assert
        Assert.Single(result);
        Assert.Equal(Coin.ETH, result[0].Coin);
        Assert.Equal(1, result[0].Amount);
        Assert.Equal("Buy", result[0].OrderType);
        Assert.Equal(100, result[0].Rate);
    }
    
    // test that the trade executor can handle no buys
    [Fact]
    public async Task ExecuteTrades_WhenNoBuys_ReturnsSellMarketOrderModels()
    {
        // Arrange
        var tradeSignals = new List<TradeSignalModel>
        {
            new()
            {
                Action = TradeAction.Sell,
                Quantity = -1,
                TickerName = Coin.BTC
            }
        };
        var portfolio = new PortfolioModel
        {
            Positions = new List<PositionModel>
            {
                new(_timeProvider.GetUtcNow())
                {
                    Name = Coin.BTC,
                    Quantity = 1
                }
            }
        };
        var priceSnapshots = new List<PriceSnapshotModel>
        {
            new()
            {
                Name = Coin.BTC,
                Bid = 1000
            }
        };

        _mockPortfolioService.Setup(m => m.GetPortfoliosAsync()).ReturnsAsync(portfolio);
        _mockPricingService.Setup(b => b.GetPriceSnapshotsAsync()).ReturnsAsync(priceSnapshots);
        _exchangeServiceMock.Setup(b => b.MarketSellAsync(Coin.BTC, 1)).ReturnsAsync(new MarketOrderModel
        {
            Coin = Coin.BTC,
            Amount = 1,
            OrderType = "Sell",
            Rate = 1000
        });

        // Act
        var result = await _tradeExecutor.ExecuteTrades(_mockPortfolioService.Object, _mockPricingService.Object, _exchangeServiceMock.Object, tradeSignals);

        // Assert
        Assert.Single(result);
        Assert.Equal(Coin.BTC, result[0].Coin);
        Assert.Equal(1, result[0].Amount);
        Assert.Equal("Sell", result[0].OrderType);
        Assert.Equal(1000, result[0].Rate);
    }
    
    // test that the trade executor can handle no portfolio
    [Fact]
    public async Task ExecuteTrades_WhenNoPortfolio_ReturnsEmptyList()
    {
        // Arrange
        var tradeSignals = new List<TradeSignalModel>
        {
            new()
            {
                Action = TradeAction.Sell,
                Quantity = -1,
                TickerName = Coin.BTC
            }
        };
        var priceSnapshots = new List<PriceSnapshotModel>
        {
            new()
            {
                Name = Coin.BTC,
                Bid = 1000
            }
        };

        _mockPortfolioService.Setup(m => m.GetPortfoliosAsync()).ReturnsAsync(new PortfolioModel());
        _mockPricingService.Setup(b => b.GetPriceSnapshotsAsync()).ReturnsAsync(priceSnapshots);

        // Act
        var result = await _tradeExecutor.ExecuteTrades(_mockPortfolioService.Object, _mockPricingService.Object, _exchangeServiceMock.Object, tradeSignals);

        // Assert
        Assert.Empty(result);
    }
    
    // test that the trade executor can handle no price snapshots
    [Fact]
    public async Task ExecuteTrades_WhenNoPriceSnapshots_ReturnsEmptyList()
    {
        // Arrange
        var tradeSignals = new List<TradeSignalModel>
        {
            new()
            {
                Action = TradeAction.Buy,
                Quantity = 1,
                TickerName = Coin.BTC
            }
        };
        var portfolio = new PortfolioModel
        {
            Positions = new List<PositionModel>
            {
                new(_timeProvider.GetUtcNow())
                {
                    Name = Coin.BTC,
                    Quantity = 1
                }
            }
        };

        _mockPortfolioService.Setup(m => m.GetPortfoliosAsync()).ReturnsAsync(portfolio);
        _mockPricingService.Setup(b => b.GetPriceSnapshotsAsync()).ReturnsAsync(new List<PriceSnapshotModel>());

        // Act
        var result = await _tradeExecutor.ExecuteTrades(_mockPortfolioService.Object, _mockPricingService.Object, _exchangeServiceMock.Object, tradeSignals);

        // Assert
        Assert.Empty(result);
    }
}