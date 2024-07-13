namespace TradingBot.Domain.Repository.Ticker;

public interface IPositionSnapshotRepository
{
    Task<List<PriceSnapshotDto>> GetDailyPrices(DateTimeOffset from, DateTimeOffset to);
    List<PriceSnapshotDto> GetPriceSnapshots(List<string> name);
    bool SavePriceSnapshots(List<PriceSnapshotDto> tickers);
}