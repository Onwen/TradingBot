using TradingBot.Domain.Service;

namespace TradingBot.Worker.workers;

public class DailyReturnCalculatorWorker(
    IServiceScopeFactory scopeFactory,TimeProvider timeProvider, ILogger<PriceFetcherWorker> logger) : SetIntervalWorker(timeProvider, logger)
{
    private readonly TimeProvider _timeProvider = timeProvider;

    protected override string WorkerName()
    {
        return nameof(DailyReturnCalculatorWorker);
    }

    public override Task<bool> ShouldExecute()
    {
        return Task.FromResult(true);
    }

    public override async Task HandleExecute()
    {
        using var scope = scopeFactory.CreateScope();
        var portfolioService = scope.ServiceProvider.GetRequiredService<IPortfolioService>();
        var pricingService = scope.ServiceProvider.GetRequiredService<IPricingService>();
        var exchangeService = scope.ServiceProvider.GetRequiredService<IExchangeService>();
        // get portfolio
        var portfolio = await portfolioService.GetPortfoliosAsync();
        // get daily prices
        DateTimeOffset yesterdayMidnight = _timeProvider.GetUtcNow().Date.AddDays(-1).ToUniversalTime();
        DateTimeOffset todayMidnight = _timeProvider.GetUtcNow().Date.ToUniversalTime();
        var dailyPrices = await pricingService.GetDailyPricesAsync(yesterdayMidnight, todayMidnight);
        // calculate previous days returns
        Dictionary<string, decimal> previousDayReturns = [];
        foreach (var position in portfolio.Positions)
        {
            var yesterdayPrice =
                dailyPrices?.FirstOrDefault(p => p.Symbol == position.Name && p.TimeClose <= todayMidnight)?.Close ??
                0;
            if (yesterdayPrice == 0)
            {
                logger.LogInformation("yesterday price is 0");
                continue;
            }
            previousDayReturns.Add(position.Name, (position.CurrentPrice / yesterdayPrice) - 1);
        }

        // TODO: save previous day returns
        await exchangeService.SaveDailyReturns(previousDayReturns);
        // log that returns were calculated
        logger.LogInformation("Calculated daily returns at: {time}", _timeProvider.GetUtcNow());
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