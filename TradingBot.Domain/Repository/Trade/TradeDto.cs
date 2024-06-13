namespace TradingBot.Domain.Repository.Trade;

public record TradeDto(string TickerFrom, decimal AmountFrom, string TickerTo, decimal AmountTo, DateTimeOffset Timestamp)
{
    public int Id { get; set; }
}