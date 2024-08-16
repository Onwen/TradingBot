using Microsoft.Extensions.Logging;
using TradingBot.Domain.Repository.Ticker;
using TradingBot.Infrastructure.Repository.DataContext;

namespace TradingBot.Infrastructure.Repository;

public class PriceSnapshotRepository(ApplicationDbContext dataContext, ILogger<PriceSnapshotRepository> logger) : IPositionSnapshotRepository
{
    public List<PriceSnapshotDto> GetPriceSnapshots(List<string> name)
    {
        logger.LogInformation("Getting tickers");
        return dataContext.Tickers.Where(t => name.Contains(t.Name)).ToList();
    }

    public List<PriceSnapshotDto> GetPriceSnapshots(List<string> name, DateTimeOffset at)
    {
        logger.LogInformation("Getting tickers");
        // select all tickers with the given name and the latest timestamp before the given timestamp
        return dataContext.Tickers
            .Where(t => name.Contains(t.Name) && t.Timestamp <= at)
            .GroupBy(t => t.Name)
            .Select(g => g.OrderByDescending(t => t.Timestamp).FirstOrDefault())
            .ToList();
    }

    public bool SavePriceSnapshots(List<PriceSnapshotDto> tickers)
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