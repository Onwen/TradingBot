namespace TradingBot.Domain.Strategy;

public interface IStrategyFactory
{
    IStrategy GetStrategy(string strategyName);
}