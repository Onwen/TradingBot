namespace TradingBot.Domain.Repository.Position;

public record PositionDto(string Exchange, string Ticker, decimal Quantity, DateTimeOffset Timestamp)
{
    public int Id { get; set; }
}