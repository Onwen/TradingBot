namespace TradingBot.Domain.Repository.Trade;

public record TradeDto(string Exchange, string TickerFrom, decimal AmountFrom, string TickerTo, decimal AmountTo, DateTime Timestamp)
{
    public int Id { get; set; }
}