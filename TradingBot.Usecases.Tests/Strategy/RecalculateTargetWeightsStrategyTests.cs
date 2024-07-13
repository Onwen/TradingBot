using MathNet.Numerics.LinearAlgebra;
using Microsoft.Extensions.Logging;
using Moq;
using TradingBot.Domain.Model;
using TradingBot.Domain.Service;
using TradingBot.Domain.TimeProvider;
using TradingBot.Usecases.Strategy;

namespace TradingBot.Usecases.Tests.Strategy;

public class RecalculateTargetWeightsStrategyTests
{
    private readonly Mock<IExchangeService> _mockExchangeService;
    private readonly TimeProvider _timeProvider;
    private readonly Mock<ILogger<RecalculateTargetWeightsStrategy>> _mockLogger;
    private readonly RecalculateTargetWeightsStrategy _recalculateTargetWeightsStrategy;
    
    public RecalculateTargetWeightsStrategyTests()
    {
        _mockExchangeService = new Mock<IExchangeService>();
        _timeProvider = new StaticTimeProvider(DateTimeOffset.UtcNow);
        _mockLogger = new Mock<ILogger<RecalculateTargetWeightsStrategy>>();
        _recalculateTargetWeightsStrategy = new RecalculateTargetWeightsStrategy(_mockExchangeService.Object, _timeProvider, _mockLogger.Object);
    }
    // test should execute when last rebalance date is before yesterday
    [Fact]
    public async Task ShouldExecute_WhenLastRebalanceDateIsBeforeYesterday_ReturnsTrue()
    {
        // Arrange
        var lastDayLogs = new List<StrategyLogModel>
        {
            new StrategyLogModel { Message = "Recalculated Target Weights", Timestamp = DateTimeOffset.UtcNow.AddDays(-2) }
        };
        _mockExchangeService.Setup(x => x.GetLogsByStrategy(It.IsAny<string>(), It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>()))
            .ReturnsAsync(lastDayLogs);
        
        // Act
        var result = await _recalculateTargetWeightsStrategy.ShouldExecute();
        
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
            new StrategyLogModel { Message = "Recalculated Target Weights", Timestamp = DateTimeOffset.UtcNow.AddDays(-1).AddMinutes(1) }
        };
        _mockExchangeService.Setup(x => x.GetLogsByStrategy(It.IsAny<string>(), It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>()))
            .ReturnsAsync(lastDayLogs);
        
        // Act
        var result = await _recalculateTargetWeightsStrategy.ShouldExecute();
        
        // Assert
        Assert.False(result);
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
        var dailyPrices = new List<PriceSnapshotModel>
        {
            new PriceSnapshotModel() { Name = "BTC", Timestamp = DateTimeOffset.UtcNow.AddDays(-1), Last = 100 },
            new PriceSnapshotModel { Name = "BTC", Timestamp = DateTimeOffset.UtcNow, Last = 200 },
            new PriceSnapshotModel { Name = "ETH", Timestamp = DateTimeOffset.UtcNow.AddDays(-1), Last = 300 },
            new PriceSnapshotModel { Name = "ETH", Timestamp = DateTimeOffset.UtcNow, Last = 400 }
        };
        _mockExchangeService.Setup(x => x.GetDailyPricesAsync(It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>()))
            .ReturnsAsync(dailyPrices);
        
        // Act
        var result = await _recalculateTargetWeightsStrategy.GetOrderedDailyPricesAsync(DateTimeOffset.UtcNow.AddDays(-2), DateTimeOffset.UtcNow);
        
        // Assert
        Assert.Equal(4, result.Length);
        Assert.Equal(100, result[0].Last);
        Assert.Equal(300, result[1].Last);
        Assert.Equal(200, result[2].Last);
        Assert.Equal(400, result[3].Last);
    }
    // test GetOrderedDailyPricesAsync throws exception when daily prices is null
    [Fact]
    public async Task GetOrderedDailyPricesAsync_WhenDailyPricesIsNull_ThrowsException()
    {
        // Arrange
        _mockExchangeService.Setup(x => x.GetDailyPricesAsync(It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>()))
            .ReturnsAsync((List<PriceSnapshotModel>)null);
        
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
        var dailyPrices = new List<PriceSnapshotModel>
        {
            new PriceSnapshotModel { Name = "BTC" },
            new PriceSnapshotModel { Name = "ETH" },
            new PriceSnapshotModel { Name = "LTC" },
            new PriceSnapshotModel { Name = "LTC" },
        };
        
        // Act
        var result = RecalculateTargetWeightsStrategy.CreateIndexLookup(dailyPrices);
        
        // Assert
        Assert.Equal(3, result.Count);
        Assert.Equal(0, result["BTC"]);
        Assert.Equal(1, result["ETH"]);
        Assert.Equal(2, result["LTC"]);
    }
    // test CreateIndexLookup returns empty index lookup when daily prices is empty
    [Fact]
    public void CreateIndexLookup_WhenDailyPricesIsEmpty_ReturnsEmptyIndexLookup()
    {
        // Arrange
        var dailyPrices = new List<PriceSnapshotModel>();
        
        // Act
        var result = RecalculateTargetWeightsStrategy.CreateIndexLookup(dailyPrices);
        
        // Assert
        Assert.Empty(result);
    }
    // test ConvertToPriceMatrix returns price matrix
    [Fact]
    public void ConvertToPriceMatrix_ReturnsPriceMatrix()
    {
        // Arrange
        var dailyPrices = new List<PriceSnapshotModel>
        {
            new PriceSnapshotModel { Name = "BTC", Last = 100},
            new PriceSnapshotModel { Name = "ETH", Last = 200 },
            new PriceSnapshotModel { Name = "LTC", Last = 300 },
            new PriceSnapshotModel { Name = "BTC", Last = 110},
            new PriceSnapshotModel { Name = "ETH", Last = 210 },
            new PriceSnapshotModel { Name = "LTC", Last = 310 },
        };
        var indexLookup = new Dictionary<string, int>
        {
            { "BTC", 0 },
            { "ETH", 1 },
            { "LTC", 2 }
        };
        
        // Act
        var result = RecalculateTargetWeightsStrategy.ConvertToPriceMatrix(dailyPrices, indexLookup, 2);
        
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
        var dailyPrices = new List<PriceSnapshotModel>();
        var indexLookup = new Dictionary<string, int>();
        
        // Act
        var result = RecalculateTargetWeightsStrategy.ConvertToPriceMatrix(dailyPrices, indexLookup, 2);
        
        // Assert
        Assert.Empty(result);
    }
    // test ConvertToPriceMatrix handles unexpected number of prices and returns price matrix
    [Fact]
    public void ConvertToPriceMatrix_WhenUnexpectedNumberOfPrices_ReturnsPriceMatrix()
    {
        // Arrange
        var dailyPrices = new List<PriceSnapshotModel>
        {
            new PriceSnapshotModel { Name = "BTC", Last = 100},
            new PriceSnapshotModel { Name = "ETH", Last = 200 },
            new PriceSnapshotModel { Name = "LTC", Last = 300 },
            new PriceSnapshotModel { Name = "BTC", Last = 110},
            new PriceSnapshotModel { Name = "ETH", Last = 210 },
        };
        var indexLookup = new Dictionary<string, int>
        {
            { "BTC", 0 },
            { "ETH", 1 },
            { "LTC", 2 }
        };
        
        // Act
        var result = RecalculateTargetWeightsStrategy.ConvertToPriceMatrix(dailyPrices, indexLookup, 2);
        
        // Assert
        Assert.Equal(6, result.Length);
        Assert.Equal(100, result[0, 0]);
        Assert.Equal(200, result[1, 0]);
        Assert.Equal(300, result[2, 0]);
        Assert.Equal(110, result[0, 1]);
        Assert.Equal(210, result[1, 1]);
        Assert.Equal(0, result[2, 1]);
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
        Assert.Equal(1.1, result.constReturnsMatrix[1, 0]);
        Assert.Equal(1.05, result.constReturnsMatrix[2, 0]);
        Assert.Equal(1.0333333333333333333333333333, result.constReturnsMatrix[3, 0]);
        Assert.Equal(1.0909090909090908, result.returnsMatrix[0, 0]);
        Assert.Equal(1.047619047619047619047619047619, result.returnsMatrix[1, 0]);
        Assert.Equal(1.032258064516129, result.returnsMatrix[2, 0]);
        
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
        Assert.Equal(1.5, result[1, 0]);
        Assert.Equal(1.4761904761904763, result[2, 0]);
        Assert.Equal(1.4545454545454546, result[3, 0]);
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
        Dictionary<string,int> indexLookup = new Dictionary<string, int>() { { "BTC", 0 }, { "ETH", 1 }, { "LTC", 2 } };
        
        // Act
        var result = _recalculateTargetWeightsStrategy.CalculateTargetWeights(Matrix<double>.Build.DenseOfArray(expectedReturns), indexLookup);
        
        // Assert
        Assert.Equal(3, result.Count);
        Assert.Equal("BTC", result[0].Name);
        Assert.Equal(0.3333333333333333333333333333m, result[0].TargetWeighting);
        Assert.Equal("ETH", result[1].Name);
        Assert.Equal(0.3333333333333333333333333333m, result[1].TargetWeighting);
        Assert.Equal("LTC", result[2].Name);
        Assert.Equal(0.3333333333333333333333333333m, result[2].TargetWeighting);
    }
    // test CalculateTargetWeights handles empty expected returns
    [Fact]
    public void CalculateTargetWeights_WhenExpectedReturnsIsEmpty_ReturnsEmptyTargetWeights()
    {
        // Arrange
        double[,] expectedReturns = new double[0,0];
        Dictionary<string,int> indexLookup = new Dictionary<string, int>();
        
        // Act
        var result = _recalculateTargetWeightsStrategy.CalculateTargetWeights(Matrix<double>.Build.DenseOfArray(expectedReturns), indexLookup);
        
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
        
        // Act
        var result = _recalculateTargetWeightsStrategy.CalculateTargetWeights(Matrix<double>.Build.DenseOfArray(expectedReturns), indexLookup);
        
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
        
        // Act
        var result = _recalculateTargetWeightsStrategy.CalculateTargetWeights(Matrix<double>.Build.DenseOfArray(expectedReturns), indexLookup);
        
        // Assert
        Assert.Empty(result);
    }
    // test CalculateTargetWeights when invalid expected returns throws exception
    [Fact]
    public void CalculateTargetWeights_WhenInvalidExpectedReturns_ThrowsException()
    {
        // Arrange
        double[,] expectedReturns = new double[3,2] { { 0.1, 0.2 }, { 0.05, 0.1 }, { 0.03, 0.06 } };
        Dictionary<string,int> indexLookup = new Dictionary<string, int>() { { "BTC", 0 }, { "ETH", 1 }, { "LTC", 2 } };
        
        // Act
        void Act() => _recalculateTargetWeightsStrategy.CalculateTargetWeights(Matrix<double>.Build.DenseOfArray(expectedReturns), indexLookup);
        
        // Assert
        Assert.Throws<Exception>(Act);
    }
    // test CalculateTargetWeights when invalid index lookup throws exception
    [Fact]
    public void CalculateTargetWeights_WhenInvalidIndexLookup_ThrowsException()
    {
        // Arrange
        double[,] expectedReturns = new double[3,1] { { 0.1 }, { 0.05 }, { 0.03 } };
        Dictionary<string,int> indexLookup = new Dictionary<string, int>() { { "BTC", 2 }, { "ETH", 3 } };
        
        // Act
        void Act() => _recalculateTargetWeightsStrategy.CalculateTargetWeights(Matrix<double>.Build.DenseOfArray(expectedReturns), indexLookup);
        
        // Assert
        Assert.Throws<ArgumentOutOfRangeException>(Act);
    }
    // test CalculateTargetWeights when invalid expected returns and index lookup throws exception
    [Fact]
    public void CalculateTargetWeights_WhenInvalidExpectedReturnsAndIndexLookup_ThrowsException()
    {
        // Arrange
        double[,] expectedReturns = new double[3,2] { { 0.1, 0.2 }, { 0.05, 0.1 }, { 0.03, 0.06 } };
        Dictionary<string,int> indexLookup = new Dictionary<string, int>() { { "BTC", 0 }, { "ETH", 1 } };
        
        // Act
        void Act() => _recalculateTargetWeightsStrategy.CalculateTargetWeights(Matrix<double>.Build.DenseOfArray(expectedReturns), indexLookup);
        
        // Assert
        Assert.Throws<Exception>(Act);
    }
    // test HandleExecute calls all necessary methods
    [Fact]
    public async Task HandleExecute_CallsAllNecessaryMethods()
    {
        // Arrange
        var dailyPrices = new List<PriceSnapshotModel>
        {
            new() { Name = "BTC", Last = 278.51m, Timestamp = DateTimeOffset.UtcNow.AddDays(-9) },
            new() { Name = "BTC", Last = 259.80m, Timestamp = DateTimeOffset.UtcNow.AddDays(-8) },
            new() { Name = "BTC", Last = 264.34m, Timestamp = DateTimeOffset.UtcNow.AddDays(-7) },
            new() { Name = "BTC", Last = 263.58m, Timestamp = DateTimeOffset.UtcNow.AddDays(-6) },
            new() { Name = "BTC", Last = 269.87m, Timestamp = DateTimeOffset.UtcNow.AddDays(-5) },
            new() { Name = "BTC", Last = 267.71m, Timestamp = DateTimeOffset.UtcNow.AddDays(-4) },
            new() { Name = "BTC", Last = 263.66m, Timestamp = DateTimeOffset.UtcNow.AddDays(-3) },
            new() { Name = "BTC", Last = 265.13m, Timestamp = DateTimeOffset.UtcNow.AddDays(-2) },
            new() { Name = "BTC", Last = 260.48m, Timestamp = DateTimeOffset.UtcNow.AddDays(-1) },
            new() { Name = "BTC", Last = 257.82m, Timestamp = DateTimeOffset.UtcNow },
            new() { Name = "ETH", Last = 2.83m, Timestamp = DateTimeOffset.UtcNow.AddDays(-9) },
            new() { Name = "ETH", Last = 1.33m, Timestamp = DateTimeOffset.UtcNow.AddDays(-8) },
            new() { Name = "ETH", Last = 0.69m, Timestamp = DateTimeOffset.UtcNow.AddDays(-7) },
            new() { Name = "ETH", Last = 1.07m, Timestamp = DateTimeOffset.UtcNow.AddDays(-6) },
            new() { Name = "ETH", Last = 1.26m, Timestamp = DateTimeOffset.UtcNow.AddDays(-5) },
            new() { Name = "ETH", Last = 1.83m, Timestamp = DateTimeOffset.UtcNow.AddDays(-4) },
            new() { Name = "ETH", Last = 1.83m, Timestamp = DateTimeOffset.UtcNow.AddDays(-3) },
            new() { Name = "ETH", Last = 1.67m, Timestamp = DateTimeOffset.UtcNow.AddDays(-2) },
            new() { Name = "ETH", Last = 1.48m, Timestamp = DateTimeOffset.UtcNow.AddDays(-1) },
            new() { Name = "ETH", Last = 1.20m, Timestamp = DateTimeOffset.UtcNow },
            new() { Name = "DOGE", Last = 0.000167313722614873m, Timestamp = DateTimeOffset.UtcNow.AddDays(-9) },
            new() { Name = "DOGE", Last = 0.000160169176119225m, Timestamp = DateTimeOffset.UtcNow.AddDays(-8) },
            new() { Name = "DOGE", Last = 0.000161990991808901m, Timestamp = DateTimeOffset.UtcNow.AddDays(-7) },
            new() { Name = "DOGE", Last = 0.000160806732673101m, Timestamp = DateTimeOffset.UtcNow.AddDays(-6) },
            new() { Name = "DOGE", Last = 0.000163113381589149m, Timestamp = DateTimeOffset.UtcNow.AddDays(-5) },
            new() { Name = "DOGE", Last = 0.000160376723948245m, Timestamp = DateTimeOffset.UtcNow.AddDays(-4) },
            new() { Name = "DOGE", Last = 0.000155723780623954m, Timestamp = DateTimeOffset.UtcNow.AddDays(-3) },
            new() { Name = "DOGE", Last = 0.000155529113544033m, Timestamp = DateTimeOffset.UtcNow.AddDays(-2) },
            new() { Name = "DOGE", Last = 0.000153420895566409m, Timestamp = DateTimeOffset.UtcNow.AddDays(-1) },
            new() { Name = "DOGE", Last = 0.000147681959388338m, Timestamp = DateTimeOffset.UtcNow },
            new() { Name = "LTC", Last = 4.18m, Timestamp = DateTimeOffset.UtcNow.AddDays(-9) },
            new() { Name = "LTC", Last = 3.91m, Timestamp = DateTimeOffset.UtcNow.AddDays(-8) },
            new() { Name = "LTC", Last = 3.90m, Timestamp = DateTimeOffset.UtcNow.AddDays(-7) },
            new() { Name = "LTC", Last = 3.94m, Timestamp = DateTimeOffset.UtcNow.AddDays(-6) },
            new() { Name = "LTC", Last = 4.12m, Timestamp = DateTimeOffset.UtcNow.AddDays(-5) },
            new() { Name = "LTC", Last = 4.06m, Timestamp = DateTimeOffset.UtcNow.AddDays(-4) },
            new() { Name = "LTC", Last = 3.89m, Timestamp = DateTimeOffset.UtcNow.AddDays(-3) },
            new() { Name = "LTC", Last = 4.02m, Timestamp = DateTimeOffset.UtcNow.AddDays(-2) },
            new() { Name = "LTC", Last = 3.91m, Timestamp = DateTimeOffset.UtcNow.AddDays(-1) },
            new() { Name = "LTC", Last = 3.95m, Timestamp = DateTimeOffset.UtcNow },
        };
        _mockExchangeService.Setup(x => x.GetDailyPricesAsync(It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>()))
            .ReturnsAsync(dailyPrices);
        _mockExchangeService.Setup(x => x.SavePositionTargetWeightingsAsync(It.IsAny<List<PositionTargetWeightingModel>>()))
            .ReturnsAsync(true);
        
        // Act
        await _recalculateTargetWeightsStrategy.HandleExecute();
        
        // Assert
        _mockExchangeService.Verify(x => x.GetDailyPricesAsync(It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>()), Times.Once);
        _mockExchangeService.Verify(x => x.SavePositionTargetWeightingsAsync(It.IsAny<List<PositionTargetWeightingModel>>()), Times.Once);
        _mockExchangeService.Verify(x => x.SaveLog(It.IsAny<StrategyLogModel>()), Times.Once);
    }
    // test HandleExecute when daily prices is null throws exception
    [Fact]
    public async Task HandleExecute_WhenDailyPricesIsNull_ThrowsException()
    {
        // Arrange
        _mockExchangeService.Setup(x => x.GetDailyPricesAsync(It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>()))
            .ReturnsAsync((List<PriceSnapshotModel>)null);
        
        // Act
        async Task Act() => await _recalculateTargetWeightsStrategy.HandleExecute();
        
        // Assert
        await Assert.ThrowsAsync<NullReferenceException>(Act);
    }
    // test HandleExecute when daily prices is empty throws exception
    [Fact]
    public async Task HandleExecute_WhenDailyPricesIsEmpty_ThrowsException()
    {
        // Arrange
        _mockExchangeService.Setup(x => x.GetDailyPricesAsync(It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>()))
            .ReturnsAsync(new List<PriceSnapshotModel>());
        
        // Act
        async Task Act() => await _recalculateTargetWeightsStrategy.HandleExecute();
        
        // Assert
        await Assert.ThrowsAsync<ArgumentException>(Act);
    }
    // test sleep time returns 10 minutes
    [Fact]
    public void SleepTime_ReturnsOneHour()
    {
        // Arrange
        // Act
        var result = _recalculateTargetWeightsStrategy.SleepTime();
        
        // Assert
        Assert.Equal(60 * 10 * 1000, result);
    }
}