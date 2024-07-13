namespace TradingBot.Domain.Repository.StrategyLog;

public record StrategyLogDto(string StrategyName, string Message, DateTimeOffset Timestamp)
{
    public int Id { get; init; }
}