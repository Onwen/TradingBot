using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TradingBot.Domain.Repository.StrategyLog;
using TradingBot.Infrastructure.Repository.DataContext;

namespace TradingBot.Infrastructure.Repository;

public class StrategyLogRepository(ApplicationDbContext dataContext, ILogger<StrategyLogRepository> logger) : IStrategyLogRepository
{  

    public async Task<List<StrategyLogDto>> GetLogs(DateTimeOffset from, DateTimeOffset to)
    {
        logger.LogInformation("Getting strategy logs");
        return await dataContext.StrategyLogs
            .Where(log => log.Timestamp >= from && log.Timestamp <= to)
            .Select(log => new StrategyLogDto(log.StrategyName, log.Message, log.Timestamp) { Id = log.Id })
            .ToListAsync();
    }

    public async Task<List<StrategyLogDto>> GetLogsByStrategy(string strategyName, DateTimeOffset from, DateTimeOffset to)
    {
        logger.LogInformation("Getting strategy logs");
        return await dataContext.StrategyLogs
            .Where(log => log.StrategyName == strategyName && log.Timestamp >= from && log.Timestamp <= to)
            .Select(log => new StrategyLogDto(log.StrategyName, log.Message, log.Timestamp) { Id = log.Id })
            .ToListAsync();
    }

    public async Task<bool> SaveLog(StrategyLogDto logDto)
    {
        logger.LogInformation("Saving strategy log");
        dataContext.StrategyLogs.Add(logDto);
        var saved = await dataContext.SaveChangesAsync();
        logger.LogInformation("Strategy log saved");
        return saved > 0;
    }
}