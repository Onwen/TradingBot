using Microsoft.Extensions.DependencyInjection;
using TradingBot.Domain.Executor;
using TradingBot.Domain.Model;
using TradingBot.Domain.Strategy;
using TradingBot.Usecases.Strategy;

namespace TradingBot.Usecases.Executor;

public class StrategyExecutor : IStrategyExecutor
{
    public async Task<List<TradeSignalModel>> GenerateSignals(IStrategyFactory strategyFactory, TimeProvider timeProvider, StrategyExecutorConfig config)
    {
        if (config?.StrategyConfigs == null || config.StrategyConfigs.Count == 0)
        {
            return [];
        }
        
        // validate strategy configs weighting sum
        var totalWeight = config.StrategyConfigs.Sum(x => x.Weight);
        if (totalWeight != 1)
        {
            throw new InvalidOperationException("Sum of strategy weights must be equal to 1");
        }
        
        var tradeSignalsLookup = new Dictionary<string, TradeSignalModel>();

        foreach (var strategyConfig in config.StrategyConfigs)
        {
            var strategy = strategyFactory.GetStrategy(strategyConfig.StrategyName);
            var generatedSignals = await strategy.GenerateSignals(strategyConfig);

            foreach (var signal in generatedSignals)
            {
                var weightedQuantity = signal.Quantity * strategyConfig.Weight;
                if (tradeSignalsLookup.TryGetValue(signal.TickerName, out var existingSignal))
                {
                    existingSignal.Quantity += weightedQuantity;
                    existingSignal.StrategyName += $";{signal.StrategyName}";
                    existingSignal.Action = existingSignal.Quantity > 0 ? TradeAction.Buy : TradeAction.Sell;
                }
                else
                {
                    tradeSignalsLookup[signal.TickerName] = new TradeSignalModel
                    {
                        TickerName = signal.TickerName,
                        Quantity = weightedQuantity,
                        Action = signal.Action,
                        StrategyName = signal.StrategyName,
                        Timestamp = signal.Timestamp
                    };
                }
            }
        }

        return tradeSignalsLookup.Values.ToList();
    }
}