namespace TradingBot.Domain.Model;

public class StrategyLogModel
{
    public string StrategyName { get; set;}
    public string Message { get; set;}
    public DateTimeOffset Timestamp { get; set;}
    public int Id { get; init;}
}