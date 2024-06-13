namespace TradingBot.Domain.Repository.Trade;

public interface ITradeRepository
{
    List<TradeDto> GetAllTrades(DateTimeOffset from, DateTimeOffset to);
    bool SaveTrades(List<TradeDto> trades);
}