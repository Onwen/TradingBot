namespace TradingBot.Domain.Repository.StrategyLog;

public interface IStrategyLogRepository
{
    // Get all strategy logs between two dates
    Task<List<StrategyLogDto>> GetLogs(DateTimeOffset from, DateTimeOffset to);
    // Get all strategy logs for a specific strategy between two dates
    Task<List<StrategyLogDto>> GetLogsByStrategy(string strategyName, DateTimeOffset from, DateTimeOffset to);
    // Save a strategy log
    Task<bool> SaveLog(StrategyLogDto log);
}