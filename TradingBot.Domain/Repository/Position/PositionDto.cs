namespace TradingBot.Domain.Repository.Position;

public record PositionDto(string Exchange, string Ticker, decimal Quantity, DateTime Timestamp)
{
    public int Id { get; set; }
}