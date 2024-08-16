using Microsoft.Extensions.DependencyInjection;
using TradingBot.Domain.Model;
using TradingBot.Domain.Strategy;

namespace TradingBot.Domain.Executor;

public interface IStrategyExecutor
{
    // function signature to generate trade signals
    Task<List<TradeSignalModel>> GenerateSignals(IStrategyFactory strategyFactory, System.TimeProvider timeProvider, StrategyExecutorConfig config);
}