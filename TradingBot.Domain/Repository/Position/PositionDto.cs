namespace TradingBot.Domain.Repository.Position;

public record PositionDto(string Ticker, decimal Quantity)
{
    public int Id { get; set; }
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
}