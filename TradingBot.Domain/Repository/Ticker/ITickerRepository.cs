namespace TradingBot.Domain.Repository.Ticker;

public interface ITickerRepository
{
    List<PriceSnapshotDto> GetTickers(List<string> name);
    bool SaveTickers(List<PriceSnapshotDto> tickers);
}