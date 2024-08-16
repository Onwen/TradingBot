namespace TradingBot.Domain.Repository.StrategyLog;

public record StrategyLogDto(string StrategyName, string Message, DateTime Timestamp)
{
    public int Id { get; init; }
}