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

public class PortfolioServiceTests
{
    private readonly Mock<IPositionRepository> _mockPositionRepository;
    private readonly Mock<IPositionSnapshotRepository> _mockTickerRepository;
    private readonly Mock<IExchangeProvider> _mockExchangeProvider;
    private readonly TimeProvider _timeProvider;
    private readonly Mock<ILogger<PortfolioService>> _mockLogger;
    private readonly PortfolioService _portfolioService;
    private readonly DateTimeOffset now = DateTimeOffset.UtcNow;
    public PortfolioServiceTests()
    {
        _mockPositionRepository = new Mock<IPositionRepository>();
        _mockTickerRepository = new Mock<IPositionSnapshotRepository>();
        _mockExchangeProvider = new Mock<IExchangeProvider>();
        _timeProvider = new StaticTimeProvider(now);
        _mockLogger = new Mock<ILogger<PortfolioService>>();
        _portfolioService = new PortfolioService(_mockPositionRepository.Object, _mockExchangeProvider.Object, _timeProvider, _mockLogger.Object);
    }
    
    #region GetPortfoliosAsync
    // Test GetPortfoliosAsync
    [Fact]
    public async void GetPortfoliosAsync_Success()
    {
        // Arrange
        var priceSnapshots = new List<PriceSnapshotModel>
        {
            new() { Name = Coin.BTC, Currency = Currency.AUD, Ask = 1000, Bid = 1001, Last = 1002 },
            new() { Name = Coin.ETH, Currency = Currency.AUD, Ask = 2000, Bid = 2001, Last = 2002 },
            new() { Name = Coin.XRP, Currency = Currency.AUD, Ask = 3000, Bid = 3001, Last = 3002 }
        };
        var positions = new List<PositionModel>
        {
            new(now) { Name = Coin.BTC, Quantity = 1 },
            new(now) { Name = Coin.ETH, Quantity = 2 },
            new(now) { Name = Coin.XRP, Quantity = 3 }
        };
        _mockExchangeProvider.Setup(x => x.GetPriceSnapshots()).ReturnsAsync(priceSnapshots);
        _mockTickerRepository.Setup(x => x.SavePriceSnapshots(It.IsAny<List<PriceSnapshotDto>>())).Returns(true);
        _mockExchangeProvider.Setup(x => x.GetPortfolio()).ReturnsAsync(positions);
        _mockPositionRepository.Setup(x => x.SavePositions(It.IsAny<string>(), It.IsAny<List<PositionDto>>())).Returns(true);
        // Act
        var result = await _portfolioService.GetPortfoliosAsync();
        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Positions.Count);
        Assert.Equal(14012, result.TotalValue);
        Assert.Equal(Coin.BTC, result.Positions[0].Name);
        Assert.Equal(Coin.ETH, result.Positions[1].Name);
        Assert.Equal(Coin.XRP, result.Positions[2].Name);
        Assert.Equal(now, result.Positions[0].Timestamp);
        Assert.Equal(now, result.Positions[1].Timestamp);
        Assert.Equal(now, result.Positions[2].Timestamp);

        _mockExchangeProvider.Verify(x => x.GetPriceSnapshots(), Times.Once);
        _mockTickerRepository.Verify(x => x.SavePriceSnapshots(It.IsAny<List<PriceSnapshotDto>>()), Times.Once);
        _mockExchangeProvider.Verify(x => x.GetPortfolio(), Times.Once);
        _mockPositionRepository.Verify(x => x.SavePositions(It.IsAny<string>(), It.IsAny<List<PositionDto>>()), Times.Once);
    }
    
    [Fact]
    public async void GetPortfoliosAsync_ExchangeProviderThrowsException()
    {
        // Arrange
        _mockExchangeProvider.Setup(x => x.GetPortfolio()).ReturnsAsync(() => throw new Exception());
        // Act
        await Assert.ThrowsAnyAsync<Exception>(() => _portfolioService.GetPortfoliosAsync());
        // Assert
        _mockExchangeProvider.Verify(x => x.GetPortfolio(), Times.Once);
        _mockPositionRepository.Verify(x => x.SavePositions(It.IsAny<string>(), It.IsAny<List<PositionDto>>()), Times.Never);
    }
    
    [Fact]
    public async void GetPortfoliosAsync_RepositoryFails()
    {
        // Arrange
        var priceSnapshots = new List<PriceSnapshotModel>
        {
            new() { Name = Coin.BTC, Currency = Currency.AUD, Ask = 1000, Bid = 1001, Last = 1002 },
            new() { Name = Coin.ETH, Currency = Currency.AUD, Ask = 2000, Bid = 2001, Last = 2002 },
            new() { Name = Coin.XRP, Currency = Currency.AUD, Ask = 3000, Bid = 3001, Last = 3002 }
        };
        var positions = new List<PositionModel>
        {
            new(now) { Name = Coin.BTC, Quantity = 1 },
            new(now) { Name = Coin.ETH, Quantity = 2 },
            new(now) { Name = Coin.XRP, Quantity = 3 }
        };
        _mockExchangeProvider.Setup(x => x.GetPriceSnapshots()).ReturnsAsync(priceSnapshots);
        _mockTickerRepository.Setup(x => x.SavePriceSnapshots(It.IsAny<List<PriceSnapshotDto>>())).Returns(true);
        _mockExchangeProvider.Setup(x => x.GetPortfolio()).ReturnsAsync(positions);
        _mockPositionRepository.Setup(x => x.SavePositions(It.IsAny<string>(), It.IsAny<List<PositionDto>>())).Returns(false);
        // Act
        await Assert.ThrowsAnyAsync<Exception>(() => _portfolioService.GetPortfoliosAsync());
        // Assert
        _mockExchangeProvider.Verify(x => x.GetPriceSnapshots(), Times.Once);
        _mockTickerRepository.Verify(x => x.SavePriceSnapshots(It.IsAny<List<PriceSnapshotDto>>()), Times.Once);
        _mockExchangeProvider.Verify(x => x.GetPortfolio(), Times.Once);
        _mockPositionRepository.Verify(x => x.SavePositions(It.IsAny<string>(), It.IsAny<List<PositionDto>>()), Times.Once);
    }
    #endregion
}