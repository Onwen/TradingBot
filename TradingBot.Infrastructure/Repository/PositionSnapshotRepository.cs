using Microsoft.Extensions.Logging;
using TradingBot.Domain.Repository.Ticker;
using TradingBot.Infrastructure.Repository.DataContext;

namespace TradingBot.Infrastructure.Repository;

public class PositionSnapshotRepository(ApplicationDbContext dataContext, ILogger<PositionSnapshotRepository> logger) : ITickerRepository
{
    public List<PriceSnapshotDto> GetTickers(List<string> name)
    {
        logger.LogInformation("Getting tickers");
        return dataContext.Tickers.Where(t => name.Contains(t.Name)).ToList();
    }

    public bool SaveTickers(List<PriceSnapshotDto> tickers)
    {
        logger.LogInformation("Saving tickers");
        var success = false;

        using var transaction = dataContext.Database.BeginTransaction();
        dataContext.Tickers.AddRange(tickers);
        success = dataContext.SaveChanges() > 0;
        if (success)
        {
            logger.LogInformation("Tickers saved");
            transaction.Commit();
        }
        else
        {
            logger.LogError("Failed to save tickers");
            transaction.Rollback();
        }

        return success;
    }
}