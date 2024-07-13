using Microsoft.Extensions.Logging;
using TradingBot.Domain.Model;
using TradingBot.Domain.Service;
using TradingBot.Domain.Strategy;

namespace TradingBot.Usecases.Strategy;

public class RebalancePortfolioStrategy(IExchangeService exchangeService, TimeProvider timeProvider, ILogger<RebalancePortfolioStrategy> logger) : IRebalancePortfolioStrategy
{    
    //create a private class to store the tickername and a related decimal amount
    private class PositionTargetModel
    {
        public string Name { get; set; }
        public decimal TargetWeighting { get; set; }
    }
    private const string Strategy = "RebalancePortfolioStrategy";
    private const string RebalancePortfolioMsg = "Rebalance Portfolio";
    public async Task<bool> ShouldExecute()
    {
        var v = RecalculateTargetWeightsStrategy.Strategy;
        // check if last portfolio rebalance was prior to recalculate target weights
        var lastDayRebalanceLogs =
            await exchangeService.GetLogsByStrategy(Strategy, timeProvider.GetUtcNow().AddDays(-1), timeProvider.GetUtcNow());
        var lastRebalanceDate = lastDayRebalanceLogs.LastOrDefault(b => b.Message == RebalancePortfolioMsg)?.Timestamp ??
                                timeProvider.GetUtcNow().AddDays(-1);
        var lastDayRecalculationLogs =
            await exchangeService.GetLogsByStrategy(RecalculateTargetWeightsStrategy.Strategy, timeProvider.GetUtcNow().AddDays(-1), timeProvider.GetUtcNow());
        var lastRecalculationDate = lastDayRecalculationLogs.LastOrDefault(b => b.Message == RecalculateTargetWeightsStrategy.RecalculatedTargetWeights)?.Timestamp ??
                                    timeProvider.GetUtcNow().AddDays(-1);
        return lastRebalanceDate < lastRecalculationDate;
    }

    public async Task HandleExecute()
    {
        // get current portfolio and calculate current weightings
        var portfolio = await exchangeService.GetPortfoliosAsync();
        var currentTotalValue = portfolio.TotalValue;
        var currentWeightings = portfolio.Positions.Select(b => new PositionTargetModel()
        {
            Name = b.Name, 
            TargetWeighting = (b.CurrentPrice * b.Quantity) / currentTotalValue
        }).ToArray();
        // get target weightings
        var targetWeightings = await exchangeService.GetPositionTargetWeightingsAsync();
        // calculate target weightings delta
        var targetWeightingsDelta = CalculateTargetWeightingsDelta(currentWeightings, targetWeightings.Select(b => new PositionTargetModel()
        {
            Name = b.Name, 
            TargetWeighting = b.TargetWeighting
        }).ToArray());
        // get current prices
        var currentPrices = await exchangeService.GetPriceSnapshotsAsync();
        // calculate target quantities
        var targetQuantities = CalculateTargetQuantities(currentPrices, targetWeightingsDelta);
        // rebalance portfolio
        await RebalancePortfolio(targetQuantities);
        // log strategy
        await exchangeService.SaveLog(new StrategyLogModel()
            { StrategyName = Strategy, Message = RebalancePortfolioMsg, Timestamp = timeProvider.GetUtcNow() });

    }
    
    //calculate position target using current weightings of type List<PositionTargetModel> and target weightings of type List<PositionTargetModel>
    private PositionTargetModel[] CalculateTargetWeightingsDelta(PositionTargetModel[] currentWeightings, PositionTargetModel[] targetWeightings)
    {
        var targetWeightingsDelta = new List<PositionTargetModel>();
        foreach (var target in targetWeightings)
        {
            var current = currentWeightings.FirstOrDefault(b => b.Name == target.Name);
            if (current == null)
            {
                targetWeightingsDelta.Add(new PositionTargetModel()
                {
                    Name = target.Name,
                    TargetWeighting = target.TargetWeighting
                });
            }
            else
            {
                targetWeightingsDelta.Add(new PositionTargetModel()
                {
                    Name = target.Name,
                    TargetWeighting = target.TargetWeighting - current.TargetWeighting
                });
            }
        }
        return targetWeightingsDelta.ToArray();
    }
    // calculate target quantities using current prices of type List<PriceSnapshotModel> and target weightings delta of type List<PositionTargetModel>
    private PositionTargetModel[] CalculateTargetQuantities(List<PriceSnapshotModel> currentPrices, PositionTargetModel[] targetWeightingsDelta)
    {
        var targetQuantities = new List<PositionTargetModel>();
        foreach (var target in targetWeightingsDelta)
        {
            var currentPrice = currentPrices.FirstOrDefault(b => b.Name == target.Name)?.Last ?? 0;
            targetQuantities.Add(new PositionTargetModel()
            {
                Name = target.Name,
                TargetWeighting = target.TargetWeighting / currentPrice
            });
        }
        return targetQuantities.ToArray();
    }
    
    // rebalance portfolio using target quantities of type List<PositionTargetModel>
    private async Task RebalancePortfolio(PositionTargetModel[] targetQuantities)
    {
        // rebalance portfolio
        foreach (var target in targetQuantities)
        {
            // buy or sell
            if (target.TargetWeighting > 0)
            {
                await exchangeService.MarketBuyAsync(target.Name, target.TargetWeighting);
            }
            else
            {
                await exchangeService.MarketSellAsync(target.Name, target.TargetWeighting);
            }
        }
    }
    

    public int SleepTime()
    {
        // sleep for next 30 minute interval
        var now = timeProvider.GetUtcNow();
        var nextThirtyMinute = new DateTimeOffset(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0, now.Offset);
        var remainder = nextThirtyMinute.Minute % 30;
        nextThirtyMinute = nextThirtyMinute.AddMinutes(30 - remainder);
        var timeToSleep = nextThirtyMinute - now;
        return (int)timeToSleep.TotalMilliseconds;
    }
}