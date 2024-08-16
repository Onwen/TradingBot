using Microsoft.Extensions.Options;
using TradingBot.Domain.Executor;
using TradingBot.Domain.Service;
using TradingBot.Domain.Strategy;

namespace TradingBot.Worker.workers;

public class StrategyExecutorWorker(
    IServiceScopeFactory scopeFactory,
    IOptions<StrategyExecutorConfig> config,
    TimeProvider timeProvider,
    ILogger<StrategyExecutorWorker> logger) : SetIntervalWorker(timeProvider, logger)
{
    private readonly TimeProvider _timeProvider = timeProvider;

    protected override string WorkerName()
    {
        return nameof(StrategyExecutorWorker);
    }

    public override Task<bool> ShouldExecute()
    {
        return Task.FromResult(true);
    }

    public override async Task HandleExecute()
    {
        var scope = scopeFactory.CreateScope();
        var strategyFactory = scope.ServiceProvider.GetRequiredService<IStrategyFactory>();
        var portfolioService = scope.ServiceProvider.GetRequiredService<IPortfolioService>();
        var pricingService = scope.ServiceProvider.GetRequiredService<IPricingService>();
        var exchangeService = scope.ServiceProvider.GetRequiredService<IExchangeService>();
        var strategyExecutor = scope.ServiceProvider.GetRequiredService<IStrategyExecutor>();
        var tradeExecutor = scope.ServiceProvider.GetRequiredService<ITradeExecutor>();
        var signals = await strategyExecutor.GenerateSignals(strategyFactory, _timeProvider, config.Value);
        var trades = await tradeExecutor.ExecuteTrades(portfolioService, pricingService, exchangeService, signals);
        // log trades
        foreach (var trade in trades)
        {
            logger.LogInformation($"Trade: {trade.Coin} {trade.Amount} {trade.OrderType} at {trade.Rate}");
        }
    }

    public override int SleepTime()
    {
        // find next day and calculate time to sleep
        var now = _timeProvider.GetUtcNow();
        var nextDay = new DateTimeOffset(now.Year, now.Month, now.Day, 0, 0, 0, now.Offset).AddDays(1);
        var timeToSleep = nextDay - now;
        return (int)timeToSleep.TotalMilliseconds;
    }
}