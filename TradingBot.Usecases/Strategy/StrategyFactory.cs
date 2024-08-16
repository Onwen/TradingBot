using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TradingBot.Domain.Strategy;

namespace TradingBot.Usecases.Strategy;

public class StrategyFactory(IServiceProvider serviceProvider, TimeProvider timeProvider, ILogger<StrategyFactory> logger) : IStrategyFactory
{
    public IStrategy GetStrategy(string strategyName)
    {
        return strategyName switch
        {
            FixedAllocationStrategy.Name => serviceProvider.GetRequiredService<FixedAllocationStrategy>(),
            VARStrategy.Name => serviceProvider.GetRequiredService<VARStrategy>(),
            _ => throw new ArgumentException("Invalid strategy name")
        };
    }
}