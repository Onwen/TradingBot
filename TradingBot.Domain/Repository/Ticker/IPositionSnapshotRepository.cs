namespace TradingBot.Domain.Repository.Ticker;

public interface IPositionSnapshotRepository
{
    List<PriceSnapshotDto> GetPriceSnapshots(List<string> name, DateTimeOffset at);
    bool SavePriceSnapshots(List<PriceSnapshotDto> tickers);
}