using TradingBot.Domain.Enum;
using TradingBot.Domain.Extension;
using TradingBot.Domain.Model;

namespace TradingBot.Domain.Tests.Extension;

public class PortfolioModelExtensionTests
{
    // Add tests for PortfolioModelExtension.ApplySignals success buy signal
    [Fact]
    public void ApplySignals_Success_Buy()
    {
        // Arrange
        var portfolioModel = new PortfolioModel
        {
            Exchange = "Exchange",
            Positions = new List<PositionModel>
            {
                new PositionModel(DateTimeOffset.UtcNow)
                {
                    Name = "AUD",
                    Quantity = 100,
                    CurrentPrice = 1
                }
            }
        };
        var prices = new List<PriceSnapshotModel>
        {
            new PriceSnapshotModel
            {
                Name = "BTC",
                Last = 100,
                Timestamp = DateTimeOffset.UtcNow
            }
        };
        var signals = new List<TradeSignalModel>
        {
            new TradeSignalModel
            {
                TickerName = "BTC",
                Action = TradeAction.Buy,
                Quantity = 1
            }
        };

        // Act
        var result = portfolioModel.ApplySignals(prices, signals);

        // Assert
        Assert.Equal(2, result.Positions.Count);
        Assert.Equal(1, result.Positions[0].Quantity);
        Assert.Equal(0, result.Positions[1].Quantity);
    }
    // Add tests for PortfolioModelExtension.ApplySignals success with sell signal
    [Fact]
    public void ApplySignals_Success_Sell()
    {
        // Arrange
        var portfolioModel = new PortfolioModel
        {
            Exchange = "Exchange",
            Positions = new List<PositionModel>
            {
                new PositionModel(DateTimeOffset.UtcNow)
                {
                    Name = "BTC",
                    Quantity = 1,
                    CurrentPrice = 100
                },
                new PositionModel(DateTimeOffset.UtcNow)
                {
                    Name = "AUD",
                    Quantity = 100,
                    CurrentPrice = 1
                }
            }
        };
        var prices = new List<PriceSnapshotModel>
        {
            new PriceSnapshotModel
            {
                Name = "BTC",
                Last = 100,
                Timestamp = DateTimeOffset.UtcNow
            }
        };
        var signals = new List<TradeSignalModel>
        {
            new TradeSignalModel
            {
                TickerName = "BTC",
                Action = TradeAction.Sell,
                Quantity = -1
            }
        };

        // Act
        var result = portfolioModel.ApplySignals(prices, signals);

        // Assert
        Assert.Equal(2, result.Positions.Count);
        Assert.Equal(0, result.Positions[0].Quantity);
        Assert.Equal(200, result.Positions[1].Quantity);
    }
    // Add tests for PortfolioModelExtension.ApplySignals success with buy and sell signal
    [Fact]
    public void ApplySignals_Success_BuyAndSell()
    {
        // Arrange
        var portfolioModel = new PortfolioModel
        {
            Exchange = "Exchange",
            Positions = new List<PositionModel>
            {
                new PositionModel(DateTimeOffset.UtcNow)
                {
                    Name = "BTC",
                    Quantity = 1,
                    CurrentPrice = 100
                },
                new PositionModel(DateTimeOffset.UtcNow)
                {
                    Name = "AUD",
                    Quantity = 100,
                    CurrentPrice = 1
                }
            }
        };
        var prices = new List<PriceSnapshotModel>
        {
            new PriceSnapshotModel
            {
                Name = "BTC",
                Last = 100,
                Timestamp = DateTimeOffset.UtcNow
            },
            new PriceSnapshotModel
            {
                Name = "ETH",
                Last = 50,
                Timestamp = DateTimeOffset.UtcNow
            }
        };
        var signals = new List<TradeSignalModel>
        {
            new TradeSignalModel
            {
                TickerName = "ETH",
                Action = TradeAction.Buy,
                Quantity = 1
            },
            new TradeSignalModel
            {
                TickerName = "BTC",
                Action = TradeAction.Sell,
                Quantity = -1
            }
        };

        // Act
        var result = portfolioModel.ApplySignals(prices, signals);

        // Assert
        Assert.Equal(3, result.Positions.Count);
        Assert.Equal(Coin.BTC, result.Positions[0].Name);
        Assert.Equal(0, result.Positions[0].Quantity);
        Assert.Equal(Coin.ETH, result.Positions[1].Name);
        Assert.Equal(1, result.Positions[1].Quantity);
        Assert.Equal(Currency.AUD.ToString(), result.Positions[2].Name);
        Assert.Equal(150, result.Positions[2].Quantity);
    }
    
    // Add tests for PortfolioModelExtension.ApplySignals success with no signals
    [Fact]
    public void ApplySignals_Success_NoSignals()
    {
        // Arrange
        var portfolioModel = new PortfolioModel
        {
            Exchange = "Exchange",
            Positions = new List<PositionModel>
            {
                new PositionModel(DateTimeOffset.UtcNow)
                {
                    Name = "BTC",
                    Quantity = 1,
                    CurrentPrice = 100
                },
                new PositionModel(DateTimeOffset.UtcNow)
                {
                    Name = "AUD",
                    Quantity = 100,
                    CurrentPrice = 1
                }
            }
        };
        var prices = new List<PriceSnapshotModel>
        {
            new PriceSnapshotModel
            {
                Name = "BTC",
                Last = 100,
                Timestamp = DateTimeOffset.UtcNow
            },
            new PriceSnapshotModel
            {
                Name = "ETH",
                Last = 50,
                Timestamp = DateTimeOffset.UtcNow
            }
        };
        var signals = new List<TradeSignalModel>();

        // Act
        var result = portfolioModel.ApplySignals(prices, signals);

        // Assert
        Assert.Equal(2, result.Positions.Count);
        Assert.Equal(1, result.Positions[0].Quantity);
        Assert.Equal(100, result.Positions[1].Quantity);
    }
    // Add tests for PortfolioModelExtension.ApplySignals success with buy and sell and no cash
    [Fact]
    public void ApplySignals_Success_BuyAndSell_NoCash()
    {
        // Arrange
        var portfolioModel = new PortfolioModel
        {
            Exchange = "Exchange",
            Positions = new List<PositionModel>
            {
                new PositionModel(DateTimeOffset.UtcNow)
                {
                    Name = "BTC",
                    Quantity = 1,
                    CurrentPrice = 100
                }
            }
        };
        var prices = new List<PriceSnapshotModel>
        {
            new PriceSnapshotModel
            {
                Name = "BTC",
                Last = 100,
                Timestamp = DateTimeOffset.UtcNow
            },
            new PriceSnapshotModel
            {
                Name = "ETH",
                Last = 50,
                Timestamp = DateTimeOffset.UtcNow
            }
        };
        var signals = new List<TradeSignalModel>
        {
            new TradeSignalModel
            {
                TickerName = "ETH",
                Action = TradeAction.Buy,
                Quantity = 2
            },
            new TradeSignalModel
            {
                TickerName = "BTC",
                Action = TradeAction.Sell,
                Quantity = -1
            }
        };

        // Act
        var result = portfolioModel.ApplySignals(prices, signals);

        // Assert
        Assert.Equal(3, result.Positions.Count);
        Assert.Equal(Coin.BTC, result.Positions[0].Name);
        Assert.Equal(0, result.Positions[0].Quantity);
        Assert.Equal(Coin.ETH, result.Positions[1].Name);
        Assert.Equal(2, result.Positions[1].Quantity);
        Assert.Equal(Currency.AUD.ToString(), result.Positions[2].Name);
        Assert.Equal(0, result.Positions[2].Quantity);
    }
    // Add tests for PortfolioModelExtension.ApplySignals failure with insufficient cash
    [Fact]
    public void ApplySignals_Failure_InsufficientCash()
    {
        // Arrange
        var portfolioModel = new PortfolioModel
        {
            Exchange = "Exchange",
            Positions = new List<PositionModel>
            {
                new PositionModel(DateTimeOffset.UtcNow)
                {
                    Name = "BTC",
                    Quantity = 1,
                    CurrentPrice = 100
                }
            }
        };
        var prices = new List<PriceSnapshotModel>
        {
            new PriceSnapshotModel
            {
                Name = "BTC",
                Last = 100,
                Timestamp = DateTimeOffset.UtcNow
            },
            new PriceSnapshotModel
            {
                Name = "ETH",
                Last = 50,
                Timestamp = DateTimeOffset.UtcNow
            }
        };
        var signals = new List<TradeSignalModel>
        {
            new TradeSignalModel
            {
                TickerName = "ETH",
                Action = TradeAction.Buy,
                Quantity = 2
            }
        };

        // Act
        var action = new Action(() => portfolioModel.ApplySignals(prices, signals));
        
        // Assert
        Assert.Throws<Exception>(action);
    }
    // Add tests for PortfolioModelExtension.ApplySignals failure where try to sell more than available
    [Fact]
    public void ApplySignals_Failure_SellMoreThanAvailable()
    {
        // Arrange
        var portfolioModel = new PortfolioModel
        {
            Exchange = "Exchange",
            Positions = new List<PositionModel>
            {
                new PositionModel(DateTimeOffset.UtcNow)
                {
                    Name = "BTC",
                    Quantity = 1,
                    CurrentPrice = 100
                }
            }
        };
        var prices = new List<PriceSnapshotModel>
        {
            new PriceSnapshotModel
            {
                Name = "BTC",
                Last = 100,
                Timestamp = DateTimeOffset.UtcNow
            },
            new PriceSnapshotModel
            {
                Name = "ETH",
                Last = 50,
                Timestamp = DateTimeOffset.UtcNow
            }
        };
        var signals = new List<TradeSignalModel>
        {
            new TradeSignalModel
            {
                TickerName = "BTC",
                Action = TradeAction.Sell,
                Quantity = 2
            }
        };

        // Act
        var action = new Action(() => portfolioModel.ApplySignals(prices, signals));
        
        // Assert
        Assert.Throws<Exception>(action);
    }
    // Add tests for PortfolioModelExtension.ApplySignals success with aud,btc, eth and sell signal for eth
    [Fact]
    public void ApplySignals_Success_AudBtcEthSellEth()
    {
        // Arrange
        var portfolioModel = new PortfolioModel
        {
            Exchange = "Exchange",
            Positions = new List<PositionModel>
            {
                new PositionModel(DateTimeOffset.UtcNow)
                {
                    Name = "BTC",
                    Quantity = 1,
                    CurrentPrice = 100
                },
                new PositionModel(DateTimeOffset.UtcNow)
                {
                    Name = "ETH",
                    Quantity = 1,
                    CurrentPrice = 50
                },
                new PositionModel(DateTimeOffset.UtcNow)
                {
                    Name = "AUD",
                    Quantity = 100,
                    CurrentPrice = 1
                }
            }
        };
        var prices = new List<PriceSnapshotModel>
        {
            new PriceSnapshotModel
            {
                Name = "BTC",
                Last = 100,
                Timestamp = DateTimeOffset.UtcNow
            },
            new PriceSnapshotModel
            {
                Name = "ETH",
                Last = 50,
                Timestamp = DateTimeOffset.UtcNow
            }
        };
        var signals = new List<TradeSignalModel>
        {
            new TradeSignalModel
            {
                TickerName = "ETH",
                Action = TradeAction.Sell,
                Quantity = -1
            }
        };

        // Act
        var result = portfolioModel.ApplySignals(prices, signals);

        // Assert
        Assert.Equal(3, result.Positions.Count);
        Assert.Equal(Coin.ETH, result.Positions[0].Name);
        Assert.Equal(0, result.Positions[0].Quantity);
        Assert.Equal(Coin.BTC, result.Positions[1].Name);
        Assert.Equal(1, result.Positions[1].Quantity);
        Assert.Equal(Currency.AUD.ToString(), result.Positions[2].Name);
        Assert.Equal(150, result.Positions[2].Quantity);
    }
    // add test for PortfolioModelExtension.ApplySignals success with aud,btc and a sell btc signal
    [Fact]
    public void ApplySignals_Success_AudBtcSellBtc()
    {
        // Arrange
        var portfolioModel = new PortfolioModel
        {
            Exchange = "Exchange",
            Positions = new List<PositionModel>
            {
                new PositionModel(DateTimeOffset.UtcNow)
                {
                    Name = "BTC",
                    Quantity = 0.0017773589824982321832847742m,
                    CurrentPrice = 44162.69163m
                },
                new PositionModel(DateTimeOffset.UtcNow)
                {
                Name = "AUD",
                Quantity = 23.834947851010046199339985570m,
                CurrentPrice = 1
                }
            }
        };
        var prices = new List<PriceSnapshotModel>
        {
            new PriceSnapshotModel
            {
                Name = "BTC",
                Last = 44162.69163m,
                Timestamp = DateTimeOffset.UtcNow
            }
        };
        var signals = new List<TradeSignalModel>
        {
            new TradeSignalModel
            {
                TickerName = "BTC",
                Action = TradeAction.Sell,
                Quantity = -0.0006184788876418669436047335m
            }
        };

        // Act
        var result = portfolioModel.ApplySignals(prices, signals);

        // Assert
        Assert.Equal(2, result.Positions.Count);
        Assert.Equal(Coin.BTC, result.Positions[0].Name);
        Assert.Equal(0.0011588800948563652396800407m, result.Positions[0].Quantity);
        Assert.Equal(Currency.AUD.ToString(), result.Positions[1].Name);
        Assert.Equal(51.148640245603233907246431738831m, result.Positions[1].Quantity);
    }
}