using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TradingBot.Domain.Repository.PriceHistory;
using TradingBot.Infrastructure.Repository.DataContext;

namespace TradingBot.Infrastructure.Repository;

public class PriceHistoryRepository(ApplicationDbContext dataContext, ILogger<PositionTargetWeightingRepository> logger) : IPriceHistoryRepository
{
    public async Task<PriceHistoryDto?> GetLatestPriceHistory(string symbol)
    {
        logger.LogInformation("Getting latest price history");
        var latestPriceHistory = await dataContext.PriceHistory
            .Where(ph => ph.Symbol == symbol)
            .OrderByDescending(ph => ph.TimeClose)
            .FirstOrDefaultAsync();

        return latestPriceHistory;
    }

    public Task<IEnumerable<PriceHistoryDto>> GetPriceHistory(string symbol)
    {
        logger.LogInformation("Getting latest price history");
        var latestPriceHistory = dataContext.PriceHistory
            .Where(ph => ph.Symbol == symbol)
            .OrderBy(ph => ph.TimeClose);

        return Task.FromResult<IEnumerable<PriceHistoryDto>>(latestPriceHistory);
    }

    public Task<IEnumerable<PriceHistoryDto>> GetPriceHistory(string symbol, DateTime start, DateTime end)
    {
        logger.LogInformation("Getting latest price history");
        var latestPriceHistory = dataContext.PriceHistory
            .Where(ph => ph.Symbol == symbol && ph.TimeOpen >= start && ph.TimeClose <= end)
            .OrderBy(ph => ph.TimeClose);

        return Task.FromResult<IEnumerable<PriceHistoryDto>>(latestPriceHistory);
    }

    public Task<IEnumerable<PriceHistoryDto>> GetPriceHistory(DateTime start, DateTime end)
    {
        logger.LogInformation("Getting latest price history");
        var latestPriceHistory = dataContext.PriceHistory
            .Where(ph => ph.TimeOpen >= start && ph.TimeClose <= end)
            .OrderBy(ph => ph.TimeClose);

        return Task.FromResult<IEnumerable<PriceHistoryDto>>(latestPriceHistory);
    }

    public Task SavePriceHistory(PriceHistoryDto priceHistory)
    {
        logger.LogInformation("Saving price history");
        var success = false;

        using var transaction = dataContext.Database.BeginTransaction();
        dataContext.PriceHistory.Add(priceHistory);
        success = dataContext.SaveChanges() > 0;
        if (success)
        {
            logger.LogInformation("Price history saved");
            transaction.Commit();
        }
        else
        {
            logger.LogError("Failed to save price history");
            transaction.Rollback();
        }

        return Task.FromResult(success);
    }

    public Task SavePriceHistory(IEnumerable<PriceHistoryDto> priceHistory)
    {
        logger.LogInformation("Saving price history");
        var success = false;

        using var transaction = dataContext.Database.BeginTransaction();
        dataContext.PriceHistory.AddRange(priceHistory);
        success = dataContext.SaveChanges() > 0;
        if (success)
        {
            logger.LogInformation("Price history saved");
            transaction.Commit();
        }
        else
        {
            logger.LogError("Failed to save price history");
            transaction.Rollback();
        }

        return Task.FromResult(success);
    }
}