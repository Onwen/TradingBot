using MathNet.Numerics.LinearAlgebra;
using Microsoft.Extensions.Logging;
using TradingBot.Domain.Math;
using TradingBot.Domain.Model;
using TradingBot.Domain.Service;
using TradingBot.Domain.Strategy;

namespace TradingBot.Usecases.Strategy;

public class RecalculateTargetWeightsStrategy(
    IExchangeService exchangeService,
    TimeProvider timeProvider,
    ILogger<RecalculateTargetWeightsStrategy> logger) : IRecalculateTargetWeightsStrategy
{
    public const string Strategy = "RecalculateTargetWeightsStrategy";
    public const string RecalculatedTargetWeights = "Recalculated Target Weights";

    public async Task<bool> ShouldExecute()
    {
        // check if last recalculation was more than 1 day ago
        var lastDayLogs =
            await exchangeService.GetLogsByStrategy(Strategy, timeProvider.GetUtcNow().AddDays(-1), timeProvider.GetUtcNow());
        var lastRebalanceDate = lastDayLogs.LastOrDefault(b => b.Message == RecalculatedTargetWeights)?.Timestamp ??
                                timeProvider.GetUtcNow().AddDays(-1);
        return timeProvider.GetUtcNow().AddDays(-1) > lastRebalanceDate;
    }

    public async Task HandleExecute()
    {
        var lookbackDays = 60;
        var (nDaysAgoMidnight, todayMidnight) = GetLookbackPeriod(lookbackDays);

        var dailyPrices = await GetOrderedDailyPricesAsync(nDaysAgoMidnight, todayMidnight);
        var priceSnapshotModels = dailyPrices as PriceSnapshotModel[] ?? dailyPrices.ToArray();
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

        var targetWeights = CalculateTargetWeights(expectedReturns, indexLookup);

        await SaveTargetWeightsAsync(targetWeights);

        await LogStrategyAsync(todayMidnight);
    }

    public (DateTimeOffset nDaysAgoMidnight, DateTimeOffset todayMidnight) GetLookbackPeriod(int lookbackDays)
    {
        var nDaysAgoMidnight = timeProvider.GetUtcNow().Date.AddDays(-lookbackDays);
        var todayMidnight = timeProvider.GetUtcNow().Date;
        return (nDaysAgoMidnight, todayMidnight);
    }

    public async Task<PriceSnapshotModel[]> GetOrderedDailyPricesAsync(DateTimeOffset from, DateTimeOffset to)
    {
        var dailyPrices = await exchangeService.GetDailyPricesAsync(from, to);
        var orderedDailyPrices = dailyPrices?.OrderBy(b => b.Timestamp).ToArray();
        if (orderedDailyPrices != null) return orderedDailyPrices;
        logger.LogError("ordered daily prices is null");
        throw new NullReferenceException("ordered daily prices is null");
    }

    public static Dictionary<string, int> CreateIndexLookup(IEnumerable<PriceSnapshotModel> dailyPrices)
    {
        var indexLookup = new Dictionary<string, int>();
        var nextIndex = 0;
        foreach (var price in dailyPrices)
        {
            if (!indexLookup.TryAdd(price.Name, nextIndex)) continue;
            nextIndex++;
        }

        return indexLookup;
    }

    public static decimal[,] ConvertToPriceMatrix(IEnumerable<PriceSnapshotModel> dailyPrices, Dictionary<string, int> indexLookup,
        int lookbackDays)
    {
        var priceMatrix = new decimal[indexLookup.Count, lookbackDays];
        var lastPos = new int[indexLookup.Count];
        foreach (var price in dailyPrices)
        {
            var index = indexLookup[price.Name];
            var pos = lastPos[index];
            lastPos[index]++;
            priceMatrix[index, pos] = price.Last;
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
                constReturnsMatrix[ticker + 1, dayPrice] = r;
            }
        }
        for (var dayPrice = 0; dayPrice < lookbackDays - 2; ++dayPrice)
        {
            for (var ticker = 0; ticker < indexCount; ++ticker)
            {
                var r = (double)(priceMatrix[ticker, dayPrice + 2] / priceMatrix[ticker, dayPrice + 1]);
                returnsMatrix[ticker, dayPrice] = r;
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
            lastReturnsMatrix[i + 1, 0] = (double)(priceMatrix[i, lookbackDays - 1] / priceMatrix[i, lookbackDays - 2]);
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

    public List<PositionTargetWeightingModel> CalculateTargetWeights(Matrix<double> expectedReturns,
        Dictionary<string, int> indexLookup)
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

        var targetWeights = indexMappings.Select(index => new PositionTargetWeightingModel(timeProvider.GetUtcNow())
        {
            Name = index.Key,
            TargetWeighting = expectedReturns[index.Value, 0] > 0 ? 1m / totalAboveZero : 0m,
            Timestamp = timeProvider.GetUtcNow()
        }).ToList();

        return targetWeights;
    }

    public async Task SaveTargetWeightsAsync(List<PositionTargetWeightingModel> targetWeights)
    {
        if (!await exchangeService.SavePositionTargetWeightingsAsync(targetWeights))
        {
            throw new Exception("Error saving calculating new target weights");
        }
    }

    public async Task LogStrategyAsync(DateTimeOffset timestamp)
    {
        await exchangeService.SaveLog(new StrategyLogModel()
        {
            StrategyName = "Strategy", // Assuming "Strategy" is a constant or field
            Message = "RecalculatedTargetWeights", // Assuming "RecalculatedTargetWeights" is a constant or field
            Timestamp = timestamp
        });
    }


    public int SleepTime()
    {
        // sleep for 10 minutes
        return 600000;
    }
}