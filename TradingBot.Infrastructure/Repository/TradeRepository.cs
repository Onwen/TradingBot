using Microsoft.Extensions.Logging;
using TradingBot.Domain.Repository.Trade;
using TradingBot.Infrastructure.Repository.DataContext;

namespace TradingBot.Infrastructure.Repository;

public class TradeRepository(ApplicationDbContext dataContext, ILogger<TradeRepository> logger) : ITradeRepository
{
    public List<TradeDto> GetAllTrades(DateTimeOffset from, DateTimeOffset to)
    {
        logger.LogInformation("Getting trades");
        return dataContext.Trades.Where(t => t.Timestamp >= from && t.Timestamp <= to).ToList();
    }

    public bool SaveTrades(List<TradeDto> trades)
    {
        logger.LogInformation("Saving trades");
        var success = false;

        using var transaction = dataContext.Database.BeginTransaction();
        dataContext.Trades.AddRange(trades);
        success = dataContext.SaveChanges() > 0;
        if (success)
        {
            logger.LogInformation("Trades saved");
            transaction.Commit();
        }
        else
        {
            logger.LogError("Failed to save trades");
            transaction.Rollback();
        }

        return success;
    }
}