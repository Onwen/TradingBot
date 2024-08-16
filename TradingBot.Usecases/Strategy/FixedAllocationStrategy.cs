using TradingBot.Domain.Executor;
using TradingBot.Domain.Model;
using TradingBot.Domain.Service;
using TradingBot.Domain.Strategy;

namespace TradingBot.Usecases.Strategy;

public class FixedAllocationStrategy(IPortfolioService portfolioService, IPricingService pricingService, TimeProvider timeProvider) : IStrategy
{
    public const string Name = "FixedAllocationStrategy";
    public async Task<List<TradeSignalModel>> GenerateSignals(StrategyConfig config)
    {
        var signals = new List<TradeSignalModel>();
        var portfolio = await portfolioService.GetPortfoliosAsync();
        var assetPrices = await pricingService.GetPriceSnapshotsAsync();
        var targetAllocations = ParseTargetAllocations(config.Config, portfolio.Positions);
        
        VerifyTargetAllocations(targetAllocations);
        
        
        var portfolioAllocations = portfolio.Positions.ToDictionary(kv => kv.Name, kv => kv.Quantity);
        
        foreach (var target in targetAllocations.Where(t => t.Key != "AUD"))
        {
            var asset = assetPrices.FirstOrDefault(b => b.Name == target.Key);
            if (asset == null) continue;
            
            var currentAllocation  = portfolioAllocations.GetValueOrDefault(target.Key, 0);
            var targetPositionSize = 0m;
            try
            {
                targetPositionSize  = portfolio.TotalValue * target.Value / asset.Last;
            }
            catch (Exception ex)
            {
                continue;
            }
            var positionDifference = targetPositionSize - currentAllocation;
            signals.Add(new TradeSignalModel()
            {
                TickerName = target.Key,
                Quantity = positionDifference  ,
                Action = positionDifference > 0 ? TradeAction.Buy : TradeAction.Sell,
                StrategyName = Name,
                Timestamp = timeProvider.GetUtcNow(),
            });
        }

        return signals;
    }

    private Dictionary<string, decimal> ParseTargetAllocations(Dictionary<string, string> config, List<PositionModel> currentPositions)
    {
        var targetPositions = config
            .Where(kv => kv.Key != "CashReservePercentage")
            .ToDictionary(kv => kv.Key, kv => decimal.Parse(kv.Value));
        // include current positions as 0 when not specified in config
        currentPositions.ForEach(pos => targetPositions.TryAdd(pos.Name,0));

        return targetPositions;
    }
    
    private void VerifyTargetAllocations(Dictionary<string, decimal> targetAllocations)
    {
        if (targetAllocations.Values.Sum() > 1)
        {
            throw new InvalidOperationException("Target allocations must sum to less than or equal to 1");
        }
    }
}