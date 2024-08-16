using TradingBot.Domain.Executor;
using TradingBot.Domain.Model;

namespace TradingBot.Domain.Strategy;

public interface IStrategy
{
    Task<List<TradeSignalModel>> GenerateSignals(StrategyConfig config);
}