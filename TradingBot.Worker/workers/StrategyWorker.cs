using TradingBot.Domain.Model;
using TradingBot.Domain.Service;
using TradingBot.Domain.Strategy;

namespace TradingBot.Worker.workers;

public class StrategyWorker<TStrategy>(IServiceScopeFactory scopeFactory, TimeProvider timeProvider, ILogger<StrategyWorker<TStrategy>> logger) : BackgroundService where TStrategy : IStrategy
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = scopeFactory.CreateScope();
            var strategy = scope.ServiceProvider.GetRequiredService<TStrategy>();
            logger.LogInformation("{worker} running at: {time}", nameof(strategy), timeProvider.GetUtcNow());
            
            if (await strategy.ShouldExecute())
            {
                await strategy.HandleExecute();
            }
            
            await Task.Delay(strategy.SleepTime(), stoppingToken);
        }
    }
}