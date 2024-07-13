using Microsoft.Extensions.Logging;
using TradingBot.Domain.Model;
using TradingBot.Domain.Service;
using TradingBot.Domain.Strategy;

namespace TradingBot.Usecases.Strategy;

public class CalculateDailyReturnsStrategy(IExchangeService exchangeService, TimeProvider timeProvider, ILogger<CalculateDailyReturnsStrategy> logger) : ICalculateDailyReturnsStrategy
{
    private const string Strategy = "CalculateDailyReturns";
    private const string CalculatedReturns = "Calculated Returns";
    public async Task<bool> ShouldExecute()
    {
        var lastDayLogs =
            await exchangeService.GetLogsByStrategy(Strategy, timeProvider.GetUtcNow().AddDays(-1), timeProvider.GetUtcNow());
        var lastRebalanceDate = lastDayLogs.LastOrDefault(b => b.Message == CalculatedReturns)?.Timestamp ??
                                timeProvider.GetUtcNow().AddDays(-1);
        return timeProvider.GetUtcNow().AddDays(-1) > lastRebalanceDate;
    }

    public async Task HandleExecute()
    {
        // get portfolio
        var portfolio = await exchangeService.GetPortfoliosAsync();
        // get daily prices
        DateTimeOffset yesterdayMidnight = timeProvider.GetUtcNow().Date.AddDays(-1).ToUniversalTime();
        DateTimeOffset todayMidnight = timeProvider.GetUtcNow().Date.ToUniversalTime();
        var dailyPrices = await exchangeService.GetDailyPricesAsync(yesterdayMidnight, todayMidnight);
        // calculate previous days returns
        Dictionary<string, decimal> previousDayReturns = [];
        foreach (var position in portfolio.Positions)
        {
            var yesterdayPrice =
                dailyPrices?.FirstOrDefault(p => p.Name == position.Name && p.Timestamp <= todayMidnight)?.Last ??
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
        // log strategy
        await exchangeService.SaveLog(new StrategyLogModel()
            { StrategyName = Strategy, Message = CalculatedReturns, Timestamp = timeProvider.GetUtcNow() });
    }

    public int SleepTime()
    {
        // sleep for 1 hour
        return 3600000;
    }
}