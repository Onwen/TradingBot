using MathNet.Numerics.LinearAlgebra;
using Microsoft.Extensions.Logging;
using Moq;
using TradingBot.Domain.Enum;
using TradingBot.Domain.Executor;
using TradingBot.Domain.Model;
using TradingBot.Domain.Service;
using TradingBot.Domain.TimeProvider;
using TradingBot.Usecases.Strategy;

namespace TradingBot.Usecases.Tests.Strategy;

public class VARStrategyTests
{
    private readonly Mock<IPortfolioService> _mockPortfolioService;
    private readonly Mock<IPricingService> _mockPricingService;
    private readonly TimeProvider _timeProvider;
    private readonly Mock<ILogger<VARStrategy>> _mockLogger;
    private readonly VARStrategy _recalculateTargetWeightsStrategy;
    
    public VARStrategyTests()
    {
        _mockPortfolioService = new Mock<IPortfolioService>();
        _mockPricingService = new Mock<IPricingService>();
        _timeProvider = new StaticTimeProvider(DateTimeOffset.UtcNow);
        _mockLogger = new Mock<ILogger<VARStrategy>>();
        _recalculateTargetWeightsStrategy = new VARStrategy(_mockPortfolioService.Object, _mockPricingService.Object, _timeProvider, _mockLogger.Object);
    }
    
    // test GetLookbackPeriod returns 1 day
    [Fact]
    public void GetLookbackPeriod_ReturnsOneDay()
    {
        // Arrange
        var nDays = 60;
        // Act
        var result = _recalculateTargetWeightsStrategy.GetLookbackPeriod(nDays);
        
        // Assert
        Assert.Equal(nDays, (result.todayMidnight - result.nDaysAgoMidnight).Days);
    }
    // test GetOrderedDailyPricesAsync returns ordered daily prices
    [Fact]
    public async Task GetOrderedDailyPricesAsync_ReturnsOrderedDailyPrices()
    {
        // Arrange
        var dailyPrices = new List<PriceHistoryModel>
        {
            new()
                { Symbol = Coin.BTC, Open = 110, Close = 110, TimeClose = DateTimeOffset.UtcNow.AddDays(-1) },
            new()
                { Symbol = Coin.ETH, Open = 210, Close = 210, TimeClose = DateTimeOffset.UtcNow.AddDays(-1) },
            new()
                { Symbol = Coin.LTC, Open = 310, Close = 310, TimeClose = DateTimeOffset.UtcNow.AddDays(-1) },
            new()
                { Symbol = Coin.BTC, Open = 100, Close = 100, TimeClose = DateTimeOffset.UtcNow.AddDays(-2) },
            new()
                { Symbol = Coin.ETH, Open = 200, Close = 200, TimeClose = DateTimeOffset.UtcNow.AddDays(-2) },
            new()
                { Symbol = Coin.LTC, Open = 300, Close = 300, TimeClose = DateTimeOffset.UtcNow.AddDays(-2) },
            new() 
                { Symbol = Coin.BTC, Open = 120, Close = 120, TimeClose = DateTimeOffset.UtcNow },
            new() 
                { Symbol = Coin.ETH, Open = 220, Close = 220, TimeClose = DateTimeOffset.UtcNow },
            new() 
                { Symbol = Coin.LTC, Open = 320, Close = 320, TimeClose = DateTimeOffset.UtcNow },
        };
        _mockPricingService.Setup(x => x.GetDailyPricesAsync(It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>()))
            .ReturnsAsync(dailyPrices);
        
        // Act
        var result = await _recalculateTargetWeightsStrategy.GetOrderedDailyPricesAsync(DateTimeOffset.UtcNow.AddDays(-2), DateTimeOffset.UtcNow);
        
        // Assert
        Assert.Equal(9, result.Length);
        Assert.Equal(Coin.BTC, result[0].Symbol);
        Assert.Equal(dailyPrices[3].TimeClose, result[0].TimeClose);
        Assert.Equal(100, result[0].Close);
        Assert.Equal(Coin.ETH, result[1].Symbol);
        Assert.Equal(dailyPrices[4].TimeClose, result[1].TimeClose);
        Assert.Equal(200, result[1].Close);
        Assert.Equal(Coin.LTC, result[2].Symbol);
        Assert.Equal(dailyPrices[5].TimeClose, result[2].TimeClose);
        Assert.Equal(300, result[2].Close);
        Assert.Equal(Coin.BTC, result[3].Symbol);
        Assert.Equal(dailyPrices[0].TimeClose, result[3].TimeClose);
        Assert.Equal(110, result[3].Close);
    }
    // test GetOrderedDailyPricesAsync throws exception when daily prices is null
    [Fact]
    public async Task GetOrderedDailyPricesAsync_WhenDailyPricesIsNull_ThrowsException()
    {
        // Arrange
        _mockPricingService.Setup(x => x.GetDailyPricesAsync(It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>()))
            .ReturnsAsync((List<PriceHistoryModel>)null);
        
        // Act
        async Task Act() => await _recalculateTargetWeightsStrategy.GetOrderedDailyPricesAsync(DateTimeOffset.UtcNow.AddDays(-2), DateTimeOffset.UtcNow);
        
        // Assert
        await Assert.ThrowsAsync<NullReferenceException>(Act);
    }
    // test CreateIndexLookup returns index lookup
    [Fact]
    public void CreateIndexLookup_ReturnsIndexLookup()
    {
        // Arrange
        var dailyPrices = new List<PriceHistoryModel>
        {
            new PriceHistoryModel { Symbol = Coin.BTC, Close = 100 },
            new PriceHistoryModel { Symbol = Coin.ETH, Close = 200 },
            new PriceHistoryModel { Symbol = Coin.LTC, Close = 300 },
            new PriceHistoryModel { Symbol = Coin.BTC, Close = 110 },
            new PriceHistoryModel { Symbol = Coin.ETH, Close = 210 },
            new PriceHistoryModel { Symbol = Coin.LTC, Close = 310 },
        };
        
        // Act
        var result = VARStrategy.CreateIndexLookup(dailyPrices);
        
        // Assert
        Assert.Equal(3, result.Count);
        Assert.Equal(0, result[Coin.BTC]);
        Assert.Equal(1, result[Coin.ETH]);
        Assert.Equal(2, result[Coin.LTC]);
    }
    // test CreateIndexLookup returns empty index lookup when daily prices is empty
    [Fact]
    public void CreateIndexLookup_WhenDailyPricesIsEmpty_ReturnsEmptyIndexLookup()
    {
        // Arrange
        var dailyPrices = new List<PriceHistoryModel>();
        
        // Act
        var result = VARStrategy.CreateIndexLookup(dailyPrices);
        
        // Assert
        Assert.Empty(result);
    }
    // test ConvertToPriceMatrix returns price matrix
    [Fact]
    public void ConvertToPriceMatrix_ReturnsPriceMatrix()
    {
        // Arrange
        var dailyPrices = new List<PriceHistoryModel>
        {
            new PriceHistoryModel { Symbol = Coin.BTC, Close = 100 },
            new PriceHistoryModel { Symbol = Coin.ETH, Close = 200 },
            new PriceHistoryModel { Symbol = Coin.LTC, Close = 300 },
            new PriceHistoryModel { Symbol = Coin.BTC, Close = 110 },
            new PriceHistoryModel { Symbol = Coin.ETH, Close = 210 },
            new PriceHistoryModel { Symbol = Coin.LTC, Close = 310 },
        };
        var indexLookup = new Dictionary<string, int>
        {
            { Coin.BTC, 0 },
            { Coin.ETH, 1 },
            { Coin.LTC, 2 }
        };
        
        // Act
        var result = VARStrategy.ConvertToPriceMatrix(dailyPrices, indexLookup, 2);
        
        // Assert
        Assert.Equal(6, result.Length);
        Assert.Equal(100, result[0, 0]);
        Assert.Equal(200, result[1, 0]);
        Assert.Equal(300, result[2, 0]);
        Assert.Equal(110, result[0, 1]);
        Assert.Equal(210, result[1, 1]);
        Assert.Equal(310, result[2, 1]);
    }
    // test ConvertToPriceMatrix handles empty daily prices
    [Fact]
    public void ConvertToPriceMatrix_WhenDailyPricesIsEmpty_ReturnsEmptyPriceMatrix()
    {
        // Arrange
        var dailyPrices = new List<PriceHistoryModel>();
        var indexLookup = new Dictionary<string, int>();
        
        // Act
        var result = VARStrategy.ConvertToPriceMatrix(dailyPrices, indexLookup, 2);
        
        // Assert
        Assert.Empty(result);
    }
    // test ConvertToPriceMatrix handles unexpected number of prices and returns price matrix
    [Fact]
    public void ConvertToPriceMatrix_WhenUnexpectedNumberOfPrices_ReturnsPriceMatrix()
    {
        // Arrange
        var dailyPrices = new List<PriceHistoryModel>
        {
            new PriceHistoryModel { Symbol = Coin.BTC, Close = 100 },
            new PriceHistoryModel { Symbol = Coin.ETH, Close = 200 },
            new PriceHistoryModel { Symbol = Coin.LTC, Close = 300 },
            new PriceHistoryModel { Symbol = Coin.BTC, Close = 110 },
            new PriceHistoryModel { Symbol = Coin.ETH, Close = 210 },
            new PriceHistoryModel { Symbol = Coin.LTC, Close = 310 },
        };
        var indexLookup = new Dictionary<string, int>
        {
            { Coin.BTC, 0 },
            { Coin.ETH, 1 },
            { Coin.LTC, 2 }
        };
        
        // Act
        var result = VARStrategy.ConvertToPriceMatrix(dailyPrices, indexLookup, 2);
        
        // Assert
        Assert.Equal(6, result.Length);
        Assert.Equal(100, result[0, 0]);
        Assert.Equal(200, result[1, 0]);
        Assert.Equal(300, result[2, 0]);
        Assert.Equal(110, result[0, 1]);
        Assert.Equal(210, result[1, 1]);
        Assert.Equal(310, result[2, 1]);
    }
    // test ValidatePriceMatrix does not throw exception when price matrix is valid
    [Fact]
    public void ValidatePriceMatrix_WhenPriceMatrixIsValid_Returns()
    {
        // Arrange
        var priceMatrix = new decimal[,]
        {
            { 100, 200, 300 },
            { 110, 210, 310 }
        };
        
        // Act
        void Act() => _recalculateTargetWeightsStrategy.ValidatePriceMatrix(priceMatrix, 2);
        
        // Assert
        Assert.Throws<Exception>(Act);
    }
    // test ValidatePriceMatrix throws exception when price matrix is invalid
    [Fact]
    public void ValidatePriceMatrix_WhenPriceMatrixIsInvalid_ThrowsException()
    {
        // Arrange
        var priceMatrix = new decimal[,]
        {
            { 100, 200, 300 },
            { 110, 210, 310 },
            { 120, 220, 320 }
        };
        
        // Act
        void Act() => _recalculateTargetWeightsStrategy.ValidatePriceMatrix(priceMatrix, 2);
        
        // Assert
        Assert.Throws<Exception>(Act);
    }
    // test CalculateReturnsMatrix returns returns matrix
    [Fact]
    public void CalculateReturnsMatrix_ReturnsReturnsMatrix()
    {
        // Arrange
        var priceMatrix = new decimal[,]
        {
            { 100, 110, 120, 130, 140 },
            { 200, 210, 220, 230, 240 },
            { 300, 310, 320, 330, 340 },
        };
        
        // Act
        var result = _recalculateTargetWeightsStrategy.CalculateReturnsMatrices(priceMatrix, 3, 5);
        
        // Assert
        Assert.Equal(9, result.returnsMatrix.Length);
        Assert.Equal(12, result.constReturnsMatrix.Length);
        Assert.Equal(0.10000000000000009, result.constReturnsMatrix[1, 0]);
        Assert.Equal(0.050000000000000044, result.constReturnsMatrix[2, 0]);
        Assert.Equal(0.033333333333333437, result.constReturnsMatrix[3, 0]);
        Assert.Equal(0.090909090909090828, result.returnsMatrix[0, 0]);
        Assert.Equal(0.047619047619047672, result.returnsMatrix[1, 0]);
        Assert.Equal(0.032258064516129004, result.returnsMatrix[2, 0]);
        
    }
    // test CalculateReturnsMatrix handles empty price matrix
    [Fact]
    public void CalculateReturnsMatrix_WhenPriceMatrixIsEmpty_ReturnsEmptyReturnsMatrix()
    {
        // Arrange
        var priceMatrix = new decimal[0, 0];
        
        // Act
        void Act() => _recalculateTargetWeightsStrategy.CalculateReturnsMatrices(priceMatrix, 0, 0);
        
        // Assert
        Assert.Throws<Exception>(Act);
    }
    // test CalculateReturnsMatrix throws exception when price matrix is invalid
    [Fact]
    public void CalculateReturnsMatrix_WhenPriceMatrixIsInvalid_ThrowsException()
    {
        // Arrange
        var priceMatrix = new decimal[,]
        {
            { 100, 200 },
            { 110, 210 }
        };
        
        // Act
        void Act() => _recalculateTargetWeightsStrategy.CalculateReturnsMatrices(priceMatrix, 2, 2);
        
        // Assert
        Assert.Throws<Exception>(Act);
    }
    // test CalculateLastReturnsMatrix returns last returns matrix
    [Fact]
    public void CalculateLastReturnsMatrix_ReturnsLastReturnsMatrix()
    {
        // Arrange
        var priceMatrix = new decimal[,]
        {
            { 100, 200, 300 },
            { 110, 210, 310 },
            { 120, 220, 320 }
        };
        
        // Act
        var result = _recalculateTargetWeightsStrategy.CalculateLastReturnsMatrix(priceMatrix,3, 3);
        
        // Assert
        Assert.Equal(4, result.Length);
        Assert.Equal(1, result[0, 0]);
        Assert.Equal(0.5, result[1, 0]);
        Assert.Equal(0.4761904761904763, result[2, 0]);
        Assert.Equal(0.4545454545454546, result[3, 0]);
    }
    // test CalculateLastReturnsMatrix handles empty price matrix
    [Fact]
    public void CalculateLastReturnsMatrix_WhenPriceMatrixIsEmpty_ReturnsEmptyLastReturnsMatrix()
    {
        // Arrange
        var priceMatrix = new decimal[0, 0];
        
        // Act
        var result = _recalculateTargetWeightsStrategy.CalculateLastReturnsMatrix(priceMatrix, 0, 0);
        
        // Assert
        Assert.Empty(result);
    }
    // test CalculateLastReturnsMatrix returns empty when price matrix is invalid
    [Fact]
    public void CalculateLastReturnsMatrix_WhenPriceMatrixIsInvalid_ReturnsEmpty()
    {
        // Arrange
        var priceMatrix = new decimal[,]
        {
            { 100, 200}
        };
        
        // Act
        var result = _recalculateTargetWeightsStrategy.CalculateLastReturnsMatrix(priceMatrix, 2, 1);
        
        // Assert
        Assert.Empty(result);
    }
    // test CalculateLastReturnsMatrix throws exception when price indexcount and lookbackdays dont match price matrix dimensions
    [Fact]
    public void CalculateLastReturnsMatrix_WhenPriceIndexCountAndLookbackDaysDontMatchPriceMatrixDimensions_ThrowsException()
    {
        // Arrange
        var priceMatrix = new decimal[,]
        {
            { 100, 200, 300 },
            { 110, 210, 310 }
        };
        
        // Act
        void Act() => _recalculateTargetWeightsStrategy.CalculateLastReturnsMatrix(priceMatrix, 2, 2);
        
        // Assert
        Assert.Throws<Exception>(Act);
    }
    // test CalculateExpectedReturns returns expected returns
    [Fact]
    public void CalculateExpectedReturns_ReturnsExpectedReturns()
    {
        // Arrange
        var constReturnsMatrix = new[,]
        {
            { 1, 1, 1 },
            { 264.34, 263.58, 269.87 },
            { 0.69, 1.07, 1.26 },
            { 3.9, 3.94, 4.12 }
        };
        var returnsMatrix = new[,]
        {
            { 263.58, 269.87, 267.71 },
            { 1.07, 1.26, 1.83 },
            { 3.94, 4.12, 4.06 }
        };
        var lastReturnsMatrix = new[,]
        {
            { 1 },
            { 267.71 },
            { 1.83 },
            { 4.06 }
        };
        
        // Act
        var result = _recalculateTargetWeightsStrategy.CalculateExpectedReturns(returnsMatrix, constReturnsMatrix, lastReturnsMatrix);
        
        // Assert
        Assert.Equal(3, result.RowCount);
        Assert.Equal(1, result.ColumnCount);
        Assert.Equal(356.39875527343565, result[0, 0]);
        Assert.Equal(2.4048065429687555, result[1, 0]);
        Assert.Equal(5.5473250000000203, result[2, 0]);
    }
    // test CalculateTargetWeights returns target weights
    [Fact]
    public void CalculateTargetWeights_ReturnsTargetWeights()
    {
        // Arrange
        double[,] expectedReturns = new double[3,1] { { 0.1 }, { 0.05 }, { 0.03 } };
        Dictionary<string,int> indexLookup = new Dictionary<string, int>() { { Coin.BTC, 0 }, { Coin.ETH, 1 }, { Coin.LTC, 2 } };
        PortfolioModel portfolio = new PortfolioModel()
            {
                Positions = new List<PositionModel>
                {
                    new PositionModel(_timeProvider.GetUtcNow()) { Name = Coin.BTC, Quantity = 0.5m, CurrentPrice = 50000},
                    new PositionModel(_timeProvider.GetUtcNow()) { Name = Coin.ETH, Quantity = 0.5m, CurrentPrice = 2000 }
                }
            };
        List<PriceSnapshotModel> prices = new List<PriceSnapshotModel>
        {
            new PriceSnapshotModel { Name = Coin.BTC, Last = 50000 },
            new PriceSnapshotModel { Name = Coin.ETH, Last = 2000 },
            new PriceSnapshotModel { Name = Coin.LTC, Last = 2 }
        };
        
        // Act
        var result = _recalculateTargetWeightsStrategy.CalculateTargetWeights(Matrix<double>.Build.DenseOfArray(expectedReturns), indexLookup, portfolio, prices);
        
        // Assert
        Assert.Equal(3, result.Count);
        Assert.Equal(Coin.BTC, result[0].TickerName);
        Assert.Equal(-0.3266666666666666666666666667m, result[0].Quantity);
        Assert.Equal(VARStrategy.Name, result[0].StrategyName);
        Assert.Equal(TradeAction.Sell, result[0].Action);
        Assert.Equal(Coin.ETH, result[1].TickerName);
        Assert.Equal(3.833333333333333333333333333m, result[1].Quantity);
        Assert.Equal(VARStrategy.Name, result[1].StrategyName);
        Assert.Equal(TradeAction.Buy, result[1].Action);
        Assert.Equal(Coin.LTC, result[2].TickerName);
        Assert.Equal(4333.333333333333333333333333m, result[2].Quantity);
        Assert.Equal(VARStrategy.Name, result[2].StrategyName);
        Assert.Equal(TradeAction.Buy, result[2].Action);
    }
    // test CalculateTargetWeights handles empty expected returns
    [Fact]
    public void CalculateTargetWeights_WhenExpectedReturnsIsEmpty_ReturnsEmptyTargetWeights()
    {
        // Arrange
        double[,] expectedReturns = new double[0,0];
        Dictionary<string,int> indexLookup = new Dictionary<string, int>();
        PortfolioModel portfolio = new PortfolioModel()
            {
                Positions = new List<PositionModel>
                {
                    new PositionModel(_timeProvider.GetUtcNow()) { Name = Coin.BTC, Quantity = 0.5m, CurrentPrice = 50000},
                    new PositionModel(_timeProvider.GetUtcNow()) { Name = Coin.ETH, Quantity = 0.5m, CurrentPrice = 2000 }
                }
            };
        List<PriceSnapshotModel> prices = new List<PriceSnapshotModel>
        {
            new PriceSnapshotModel { Name = Coin.BTC, Last = 50000 },
            new PriceSnapshotModel { Name = Coin.ETH, Last = 2000 },
            new PriceSnapshotModel { Name = Coin.LTC, Last = 2 }
        };
        
        // Act
        var result = _recalculateTargetWeightsStrategy.CalculateTargetWeights(Matrix<double>.Build.DenseOfArray(expectedReturns), indexLookup, portfolio, prices);
        
        // Assert
        Assert.Empty(result);
    }
    // test CalculateTargetWeights handles empty index lookup
    [Fact]
    public void CalculateTargetWeights_WhenIndexLookupIsEmpty_ReturnsEmptyTargetWeights()
    {
        // Arrange
        double[,] expectedReturns = new double[3,1] { { 0.1 }, { 0.05 }, { 0.03 } };
        Dictionary<string,int> indexLookup = new Dictionary<string, int>();
        PortfolioModel portfolio = new PortfolioModel()
            {
                Positions = new List<PositionModel>
                {
                    new PositionModel(_timeProvider.GetUtcNow()) { Name = Coin.BTC, Quantity = 0.5m, CurrentPrice = 50000},
                    new PositionModel(_timeProvider.GetUtcNow()) { Name = Coin.ETH, Quantity = 0.5m, CurrentPrice = 2000 }
                }
            };
        List<PriceSnapshotModel> prices = new List<PriceSnapshotModel>
        {
            new PriceSnapshotModel { Name = Coin.BTC, Last = 50000 },
            new PriceSnapshotModel { Name = Coin.ETH, Last = 2000 },
            new PriceSnapshotModel { Name = Coin.LTC, Last = 2 }
        };
        
        // Act
        var result = _recalculateTargetWeightsStrategy.CalculateTargetWeights(Matrix<double>.Build.DenseOfArray(expectedReturns), indexLookup, portfolio, prices);
        
        // Assert
        // TODO: should this throw an error instead
        Assert.Empty(result);
    }
    // test CalculateTargetWeights handles empty expected returns and index lookup
    [Fact]
    public void CalculateTargetWeights_WhenExpectedReturnsAndIndexLookupAreEmpty_ReturnsEmptyTargetWeights()
    {
        // Arrange
        double[,] expectedReturns = new double[0,0];
        Dictionary<string,int> indexLookup = new Dictionary<string, int>();
        PortfolioModel portfolio = new PortfolioModel()
            {
                Positions = new List<PositionModel>
                {
                    new PositionModel(_timeProvider.GetUtcNow()) { Name = Coin.BTC, Quantity = 0.5m, CurrentPrice = 50000},
                    new PositionModel(_timeProvider.GetUtcNow()) { Name = Coin.ETH, Quantity = 0.5m, CurrentPrice = 2000 }
                }
            };
        List<PriceSnapshotModel> prices = new List<PriceSnapshotModel>
        {
            new PriceSnapshotModel { Name = Coin.BTC, Last = 50000 },
            new PriceSnapshotModel { Name = Coin.ETH, Last = 2000 },
            new PriceSnapshotModel { Name = Coin.LTC, Last = 2 }
        };
        
        // Act
        var result = _recalculateTargetWeightsStrategy.CalculateTargetWeights(Matrix<double>.Build.DenseOfArray(expectedReturns), indexLookup, portfolio, prices);
        
        // Assert
        Assert.Empty(result);
    }
    // test CalculateTargetWeights when invalid expected returns throws exception
    [Fact]
    public void CalculateTargetWeights_WhenInvalidExpectedReturns_ThrowsException()
    {
        // Arrange
        double[,] expectedReturns = new double[3,2] { { 0.1, 0.2 }, { 0.05, 0.1 }, { 0.03, 0.06 } };
        Dictionary<string,int> indexLookup = new Dictionary<string, int>() { { Coin.BTC, 0 }, { Coin.ETH, 1 }, { Coin.LTC, 2 } };
        PortfolioModel portfolio = new PortfolioModel()
            {
                Positions = new List<PositionModel>
                {
                    new PositionModel(_timeProvider.GetUtcNow()) { Name = Coin.BTC, Quantity = 0.5m, CurrentPrice = 50000},
                    new PositionModel(_timeProvider.GetUtcNow()) { Name = Coin.ETH, Quantity = 0.5m, CurrentPrice = 2000 }
                }
            };
        List<PriceSnapshotModel> prices = new List<PriceSnapshotModel>
        {
            new PriceSnapshotModel { Name = Coin.BTC, Last = 50000 },
            new PriceSnapshotModel { Name = Coin.ETH, Last = 2000 },
            new PriceSnapshotModel { Name = Coin.LTC, Last = 2 }
        };
        
        // Act
        void Act() => _recalculateTargetWeightsStrategy.CalculateTargetWeights(Matrix<double>.Build.DenseOfArray(expectedReturns), indexLookup, portfolio, prices);
        
        // Assert
        Assert.Throws<Exception>(Act);
    }
    // test CalculateTargetWeights when invalid index lookup throws exception
    [Fact]
    public void CalculateTargetWeights_WhenInvalidIndexLookup_ThrowsException()
    {
        // Arrange
        double[,] expectedReturns = new double[3,1] { { 0.1 }, { 0.05 }, { 0.03 } };
        Dictionary<string,int> indexLookup = new Dictionary<string, int>() { { Coin.BTC, 2 }, { Coin.ETH, 3 } };
        PortfolioModel portfolio = new PortfolioModel()
            {
                Positions = new List<PositionModel>
                {
                    new PositionModel(_timeProvider.GetUtcNow()) { Name = Coin.BTC, Quantity = 0.5m, CurrentPrice = 50000},
                    new PositionModel(_timeProvider.GetUtcNow()) { Name = Coin.ETH, Quantity = 0.5m, CurrentPrice = 2000 }
                }
            };
        List<PriceSnapshotModel> prices = new List<PriceSnapshotModel>
        {
            new PriceSnapshotModel { Name = Coin.BTC, Last = 50000 },
            new PriceSnapshotModel { Name = Coin.ETH, Last = 2000 },
            new PriceSnapshotModel { Name = Coin.LTC, Last = 2 }
        };
        
        // Act
        void Act() => _recalculateTargetWeightsStrategy.CalculateTargetWeights(Matrix<double>.Build.DenseOfArray(expectedReturns), indexLookup, portfolio, prices);
        
        // Assert
        Assert.Throws<ArgumentOutOfRangeException>(Act);
    }
    // test CalculateTargetWeights when invalid expected returns and index lookup throws exception
    [Fact]
    public void CalculateTargetWeights_WhenInvalidExpectedReturnsAndIndexLookup_ThrowsException()
    {
        // Arrange
        double[,] expectedReturns = new double[3,2] { { 0.1, 0.2 }, { 0.05, 0.1 }, { 0.03, 0.06 } };
        Dictionary<string,int> indexLookup = new Dictionary<string, int>() { { Coin.BTC, 0 }, { Coin.ETH, 1 } };
        PortfolioModel portfolio = new PortfolioModel()
            {
                Positions = new List<PositionModel>
                {
                    new PositionModel(_timeProvider.GetUtcNow()) { Name = Coin.BTC, Quantity = 0.5m, CurrentPrice = 50000},
                    new PositionModel(_timeProvider.GetUtcNow()) { Name = Coin.ETH, Quantity = 0.5m, CurrentPrice = 2000 }
                }
            };
        List<PriceSnapshotModel> prices = new List<PriceSnapshotModel>
        {
            new PriceSnapshotModel { Name = Coin.BTC, Last = 50000 },
            new PriceSnapshotModel { Name = Coin.ETH, Last = 2000 },
            new PriceSnapshotModel { Name = Coin.LTC, Last = 2 }
        };
        
        // Act
        void Act() => _recalculateTargetWeightsStrategy.CalculateTargetWeights(Matrix<double>.Build.DenseOfArray(expectedReturns), indexLookup, portfolio, prices);
        
        // Assert
        Assert.Throws<Exception>(Act);
    }
    // test HandleExecute calls all necessary methods
    [Fact]
    public async Task HandleExecute_CallsAllNecessaryMethods()
    {
        // Arrange
        var config = new StrategyConfig();
        PortfolioModel portfolio = new PortfolioModel(){
            Positions = new List<PositionModel>
            {
                new PositionModel(_timeProvider.GetUtcNow()) { Name = Coin.BTC, Quantity = 0.5m, CurrentPrice = 257.8158m},
                new PositionModel(_timeProvider.GetUtcNow()) { Name = Coin.ETH, Quantity = 0.5m, CurrentPrice = 1.20387143115441m }
            }
        };
        var dailyPrices = new List<PriceHistoryModel>
        {
            new() { Symbol = Coin.BTC, Close = 278.51m, TimeClose = DateTimeOffset.UtcNow.AddDays(-9) },
            new() { Symbol = Coin.BTC, Close = 259.80m, TimeClose = DateTimeOffset.UtcNow.AddDays(-8) },
            new() { Symbol = Coin.BTC, Close = 264.34m, TimeClose = DateTimeOffset.UtcNow.AddDays(-7) },
            new() { Symbol = Coin.BTC, Close = 263.58m, TimeClose = DateTimeOffset.UtcNow.AddDays(-6) },
            new() { Symbol = Coin.BTC, Close = 269.87m, TimeClose = DateTimeOffset.UtcNow.AddDays(-5) },
            new() { Symbol = Coin.BTC, Close = 267.71m, TimeClose = DateTimeOffset.UtcNow.AddDays(-4) },
            new() { Symbol = Coin.BTC, Close = 263.66m, TimeClose = DateTimeOffset.UtcNow.AddDays(-3) },
            new() { Symbol = Coin.BTC, Close = 265.13m, TimeClose = DateTimeOffset.UtcNow.AddDays(-2) },
            new() { Symbol = Coin.BTC, Close = 260.48m, TimeClose = DateTimeOffset.UtcNow.AddDays(-1) },
            new() { Symbol = Coin.BTC, Close = 257.82m, TimeClose = DateTimeOffset.UtcNow },
            new() { Symbol = Coin.ETH, Close = 2.83m, TimeClose = DateTimeOffset.UtcNow.AddDays(-9) },
            new() { Symbol = Coin.ETH, Close = 1.33m, TimeClose = DateTimeOffset.UtcNow.AddDays(-8) },
            new() { Symbol = Coin.ETH, Close = 0.69m, TimeClose = DateTimeOffset.UtcNow.AddDays(-7) },
            new() { Symbol = Coin.ETH, Close = 1.07m, TimeClose = DateTimeOffset.UtcNow.AddDays(-6) },
            new() { Symbol = Coin.ETH, Close = 1.26m, TimeClose = DateTimeOffset.UtcNow.AddDays(-5) },
            new() { Symbol = Coin.ETH, Close = 1.83m, TimeClose = DateTimeOffset.UtcNow.AddDays(-4) },
            new() { Symbol = Coin.ETH, Close = 1.83m, TimeClose = DateTimeOffset.UtcNow.AddDays(-3) },
            new() { Symbol = Coin.ETH, Close = 1.67m, TimeClose = DateTimeOffset.UtcNow.AddDays(-2) },
            new() { Symbol = Coin.ETH, Close = 1.48m, TimeClose = DateTimeOffset.UtcNow.AddDays(-1) },
            new() { Symbol = Coin.ETH, Close = 1.20m, TimeClose = DateTimeOffset.UtcNow },
            new() { Symbol = Coin.DOGE, Close = 0.000167313722614873m, TimeClose = DateTimeOffset.UtcNow.AddDays(-9) },
            new() { Symbol = Coin.DOGE, Close = 0.000160169176119225m, TimeClose = DateTimeOffset.UtcNow.AddDays(-8) },
            new() { Symbol = Coin.DOGE, Close = 0.000161990991808901m, TimeClose = DateTimeOffset.UtcNow.AddDays(-7) },
            new() { Symbol = Coin.DOGE, Close = 0.000160806732673101m, TimeClose = DateTimeOffset.UtcNow.AddDays(-6) },
            new() { Symbol = Coin.DOGE, Close = 0.000163113381589149m, TimeClose = DateTimeOffset.UtcNow.AddDays(-5) },
            new() { Symbol = Coin.DOGE, Close = 0.000160376723948245m, TimeClose = DateTimeOffset.UtcNow.AddDays(-4) },
            new() { Symbol = Coin.DOGE, Close = 0.000155723780623954m, TimeClose = DateTimeOffset.UtcNow.AddDays(-3) },
            new() { Symbol = Coin.DOGE, Close = 0.000155529113544033m, TimeClose = DateTimeOffset.UtcNow.AddDays(-2) },
            new() { Symbol = Coin.DOGE, Close = 0.000153420895566409m, TimeClose = DateTimeOffset.UtcNow.AddDays(-1) },
            new() { Symbol = Coin.DOGE, Close = 0.000147681959388338m, TimeClose = DateTimeOffset.UtcNow },
            new() { Symbol = Coin.LTC, Close = 4.18m, TimeClose = DateTimeOffset.UtcNow.AddDays(-9) },
            new() { Symbol = Coin.LTC, Close = 3.91m, TimeClose = DateTimeOffset.UtcNow.AddDays(-8) },
            new() { Symbol = Coin.LTC, Close = 3.90m, TimeClose = DateTimeOffset.UtcNow.AddDays(-7) },
            new() { Symbol = Coin.LTC, Close = 3.94m, TimeClose = DateTimeOffset.UtcNow.AddDays(-6) },
            new() { Symbol = Coin.LTC, Close = 4.12m, TimeClose = DateTimeOffset.UtcNow.AddDays(-5) },
            new() { Symbol = Coin.LTC, Close = 4.06m, TimeClose = DateTimeOffset.UtcNow.AddDays(-4) },
            new() { Symbol = Coin.LTC, Close = 3.89m, TimeClose = DateTimeOffset.UtcNow.AddDays(-3) },
            new() { Symbol = Coin.LTC, Close = 4.02m, TimeClose = DateTimeOffset.UtcNow.AddDays(-2) },
            new() { Symbol = Coin.LTC, Close = 3.91m, TimeClose = DateTimeOffset.UtcNow.AddDays(-1) },
            new() { Symbol = Coin.LTC, Close = 3.95m, TimeClose = DateTimeOffset.UtcNow },
        };
        var prices = new List<PriceSnapshotModel>
        {
            new PriceSnapshotModel { Name = Coin.BTC, Last = 257.82m },
            new PriceSnapshotModel { Name = Coin.ETH, Last = 1.20m },
            new PriceSnapshotModel { Name = Coin.LTC, Last = 3.95m },
            new PriceSnapshotModel { Name = Coin.DOGE, Last = 0.000147681959388338m }
        };
        _mockPortfolioService.Setup(x => x.GetPortfoliosAsync()).ReturnsAsync(portfolio);
        _mockPricingService.Setup(x => x.GetDailyPricesAsync(It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>()))
            .ReturnsAsync(dailyPrices);
        _mockPricingService.Setup(x => x.GetPriceSnapshotsAsync()).ReturnsAsync(prices);
        
        // Act
        var signals = await _recalculateTargetWeightsStrategy.GenerateSignals(config);
        
        // Assert
        _mockPortfolioService.Verify(x => x.GetPortfoliosAsync(), Times.Once);
        _mockPricingService.Verify(x => x.GetDailyPricesAsync(It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>()), Times.Once);
        Assert.Equal(3, signals.Count);
        Assert.Equal(Coin.BTC, signals[0].TickerName);
        Assert.Equal(-0.5m, signals[0].Quantity);
        Assert.Equal(TradeAction.Sell, signals[0].Action);
        Assert.Equal(VARStrategy.Name, signals[0].StrategyName);
        Assert.Equal(Coin.ETH, signals[1].TickerName);
        Assert.Equal(-0.5m, signals[1].Quantity);
        Assert.Equal(TradeAction.Sell, signals[1].Action);
        Assert.Equal(VARStrategy.Name, signals[1].StrategyName);
        Assert.Equal(Coin.DOGE, signals[2].TickerName);
        Assert.Equal(876950.9576658840502105332968m, signals[2].Quantity);
        Assert.Equal(TradeAction.Buy, signals[2].Action);
        Assert.Equal(VARStrategy.Name, signals[2].StrategyName);
    }
    // test HandleExecute when daily prices is null throws exception
    [Fact]
    public async Task HandleExecute_WhenDailyPricesIsNull_ThrowsException()
    {
        // Arrange
        var config = new StrategyConfig();
        _mockPricingService.Setup(x => x.GetDailyPricesAsync(It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>()))!
            .ReturnsAsync((List<PriceHistoryModel>)null!);
        
        // Act
        async Task Act() => await _recalculateTargetWeightsStrategy.GenerateSignals(config);
        
        // Assert
        await Assert.ThrowsAsync<NullReferenceException>(Act);
    }
    // test HandleExecute when daily prices is empty throws exception
    [Fact]
    public async Task HandleExecute_WhenDailyPricesIsEmpty_ThrowsException()
    {
        // Arrange
        var config = new StrategyConfig();
        _mockPricingService.Setup(x => x.GetDailyPricesAsync(It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>()))
            .ReturnsAsync(new List<PriceHistoryModel>());
        
        // Act
        async Task Act() => await _recalculateTargetWeightsStrategy.GenerateSignals(config);
        
        // Assert
        await Assert.ThrowsAsync<ArgumentException>(Act);
    }
}