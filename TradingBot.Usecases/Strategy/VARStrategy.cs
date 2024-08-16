using MathNet.Numerics.LinearAlgebra;
using Microsoft.Extensions.Logging;
using TradingBot.Domain.Executor;
using TradingBot.Domain.Math;
using TradingBot.Domain.Model;
using TradingBot.Domain.Service;
using TradingBot.Domain.Strategy;

namespace TradingBot.Usecases.Strategy;

public class VARStrategy(
    IPortfolioService portfolioService,
    IPricingService pricingService,
    TimeProvider timeProvider,
    ILogger<VARStrategy> logger) : IStrategy
{
    public const string Name = "VARStrategy";
    public async Task<List<TradeSignalModel>> GenerateSignals(StrategyConfig config)
    {
        var lookbackDays = 60;
        var (nDaysAgoMidnight, todayMidnight) = GetLookbackPeriod(lookbackDays);

        var portfolio = await portfolioService.GetPortfoliosAsync();
        var dailyPrices = await GetOrderedDailyPricesAsync(nDaysAgoMidnight, todayMidnight);
        var priceSnapshotModels = dailyPrices;
        if (priceSnapshotModels.Length == 0)
        {
            throw new ArgumentException("No Price Snapshots to analyse!");
        }
        var indexLookup = CreateIndexLookup(priceSnapshotModels);
        lookbackDays = priceSnapshotModels.Length / indexLookup.Count;
        var priceMatrix = ConvertToPriceMatrix(priceSnapshotModels, indexLookup, lookbackDays);

        ValidatePriceMatrix(priceMatrix, lookbackDays);

        var (returnsMatrix, constReturnsMatrix) =
            CalculateReturnsMatrices(priceMatrix, indexLookup.Count, lookbackDays);
        var lastReturnsMatrix = CalculateLastReturnsMatrix(priceMatrix, indexLookup.Count, lookbackDays);

        var expectedReturns = CalculateExpectedReturns(returnsMatrix, constReturnsMatrix, lastReturnsMatrix);

        var prices = await pricingService.GetPriceSnapshotsAsync();
        return CalculateTargetWeights(expectedReturns, indexLookup, portfolio, prices); 
    }

    public (DateTimeOffset nDaysAgoMidnight, DateTimeOffset todayMidnight) GetLookbackPeriod(int lookbackDays)
    {
        var nDaysAgoMidnight = timeProvider.GetUtcNow().Date.AddDays(-lookbackDays);
        var todayMidnight = timeProvider.GetUtcNow().Date;
        return (nDaysAgoMidnight, todayMidnight);
    }

    public async Task<PriceHistoryModel[]> GetOrderedDailyPricesAsync(DateTimeOffset from, DateTimeOffset to)
    {
        var dailyPrices = await pricingService.GetDailyPricesAsync(from, to);
        var orderedDailyPrices = dailyPrices?.OrderBy(b => b.TimeClose).ToArray();
        if (orderedDailyPrices != null) return orderedDailyPrices;
        logger.LogError("ordered daily prices is null");
        throw new NullReferenceException("ordered daily prices is null");
    }

    public static Dictionary<string, int> CreateIndexLookup(IEnumerable<PriceHistoryModel> dailyPrices)
    {
        var indexLookup = new Dictionary<string, int>();
        var nextIndex = 0;
        foreach (var price in dailyPrices)
        {
            if (!indexLookup.TryAdd(price.Symbol, nextIndex)) continue;
            nextIndex++;
        }

        return indexLookup;
    }

    public static decimal[,] ConvertToPriceMatrix(IEnumerable<PriceHistoryModel> dailyPrices, Dictionary<string, int> indexLookup,
        int lookbackDays)
    {
        var priceMatrix = new decimal[indexLookup.Count, lookbackDays];
        var lastPos = new int[indexLookup.Count];
        foreach (var price in dailyPrices)
        {
            var index = indexLookup[price.Symbol];
            var pos = lastPos[index];
            lastPos[index]++;
            priceMatrix[index, pos] = price.Close;
        }

        return priceMatrix;
    }

    public void ValidatePriceMatrix(decimal[,] priceMatrix, int lookbackDays)
    {
        var lastPos = Enumerable.Range(0, priceMatrix.GetLength(0)).Select(i => priceMatrix.GetLength(1)).ToArray();
        if (lastPos.All(b => b == lookbackDays)) return;
        logger.LogError("Unexpected number of prices");
        throw new Exception("Unexpected number of prices");
    }

    public (double[,] returnsMatrix, double[,] constReturnsMatrix) CalculateReturnsMatrices(decimal[,] priceMatrix,
        int indexCount, int lookbackDays)
    {
        if (lookbackDays < 3)
            throw new Exception("Error with size of matrix");
        var returnsMatrix = new double[indexCount, lookbackDays - 2];
        var constReturnsMatrix = new double[indexCount + 1, lookbackDays - 2];
        for (var dayPrice = 0; dayPrice < lookbackDays - 2; ++dayPrice)
        {
            constReturnsMatrix[0, dayPrice] = 1;
            for (var ticker = 0; ticker < indexCount; ++ticker)
            {
                var r = (double)(priceMatrix[ticker, dayPrice + 1] / priceMatrix[ticker, dayPrice]);
                constReturnsMatrix[ticker + 1, dayPrice] = r - 1d;
            }
        }
        for (var dayPrice = 0; dayPrice < lookbackDays - 2; ++dayPrice)
        {
            for (var ticker = 0; ticker < indexCount; ++ticker)
            {
                var r = (double)(priceMatrix[ticker, dayPrice + 2] / priceMatrix[ticker, dayPrice + 1]);
                returnsMatrix[ticker, dayPrice] = r - 1d;
            }
        }

        return (returnsMatrix, constReturnsMatrix);
    }

    public double[,] CalculateLastReturnsMatrix(decimal[,] priceMatrix, int indexCount, int lookbackDays)
    {
        if (indexCount < 1 || lookbackDays < 2 || priceMatrix == null || priceMatrix.Length == 0)
        {
            return new double[0,0];
        }

        if (priceMatrix.Length != indexCount * lookbackDays || priceMatrix.GetLength(0) != indexCount || priceMatrix.GetLength(1) != lookbackDays)
            throw new Exception("Invalid inputs");
        var lastReturnsMatrix = new double[indexCount + 1, 1];
        lastReturnsMatrix[0, 0] = 1;
        for (var i = 0; i < indexCount; i++)
        {
            lastReturnsMatrix[i + 1, 0] = (double)(priceMatrix[i, lookbackDays - 1] / priceMatrix[i, lookbackDays - 2]) - 1d;
        }

        return lastReturnsMatrix;
    }

    public Matrix<double> CalculateExpectedReturns(double[,] returnsMatrix, double[,] constReturnsMatrix,
        double[,] lastReturnsMatrix)
    {
        var returnsMat = Matrix<double>.Build.DenseOfArray(returnsMatrix).Transpose();
        var constReturnsMat = Matrix<double>.Build.DenseOfArray(constReturnsMatrix).Transpose();
        var lastReturnsMat = Matrix<double>.Build.DenseOfArray(lastReturnsMatrix).Transpose();

        var inverseProduct = MathExtension.MatrixInverse(MathExtension.MatrixMultiply(constReturnsMat.Transpose(), constReturnsMat));
        var regressionCoefficients = MathExtension.MatrixMultiply(inverseProduct, constReturnsMat.Transpose());
        var autoRegressions = MathExtension.MatrixMultiply(regressionCoefficients, returnsMat);
        var expectedReturns = MathExtension.MatrixMultiply(lastReturnsMat, autoRegressions);

        return expectedReturns.Transpose();
    }

    public List<TradeSignalModel> CalculateTargetWeights(Matrix<double> expectedReturns,
        Dictionary<string, int> indexLookup, PortfolioModel portfolio, List<PriceSnapshotModel> prices)
    {
        if (expectedReturns.ColumnCount > 1)
            throw new Exception("Invalid number of columns");
        var indexMappings = indexLookup.ToList().OrderBy(kvp => kvp.Value);
        var totalAboveZero = 0;
        for (var i = 0; i < expectedReturns.RowCount; ++i)
        {
            if (expectedReturns[i, 0] > 0)
                totalAboveZero++;
        }

        var tradeSignals = new List<TradeSignalModel>();
        var positions = portfolio.Positions.ToDictionary(kv => kv.Name, kv => kv.CurrentPrice);
        foreach (var index in indexMappings)
        {
            var ownedQuantity = portfolio.Positions.FirstOrDefault(b => b.Name == index.Key)?.Quantity ?? 0;
            var price = prices.FirstOrDefault(b=> b.Name == index.Key)?.Last ?? 0;
            if (price == 0)
            {
                logger.LogError("Price is 0 for {index}", index.Key);
                continue;
            }
            var total = portfolio.TotalValue;
            var weighting = expectedReturns[index.Value, 0] > 0 ? 1m / totalAboveZero : 0m;
            var quantity = (weighting * total / price) - ownedQuantity;
            if (quantity == 0) continue;
            tradeSignals.Add(new TradeSignalModel()
            {
                StrategyName = Name,
                TickerName = index.Key,
                Quantity = quantity,
                Timestamp = timeProvider.GetUtcNow(),
                Action = quantity > 0 ? TradeAction.Buy : TradeAction.Sell
            });
        }

        return tradeSignals;
    }
}