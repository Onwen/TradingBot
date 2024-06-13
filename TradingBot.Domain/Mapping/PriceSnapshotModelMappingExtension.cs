using TradingBot.Domain.Enum;
using TradingBot.Domain.Model;
using TradingBot.Domain.Repository.Ticker;

namespace TradingBot.Domain.Mapping;

public static class PriceSnapshotModelMappingExtension
{
    // map price snapshot model to price snapshot dto
    public static PriceSnapshotDto MapToPriceSnapshotDto(this PriceSnapshotModel priceSnapshot)
    {
        if (priceSnapshot == null)
        {
            return null;
        }
        return new PriceSnapshotDto(priceSnapshot.Name, Currency.AUD,priceSnapshot.Bid, priceSnapshot.Ask, priceSnapshot.Last);
    }
    // map list of price snapshot model to list of price snapshot dto
    public static List<PriceSnapshotDto> MapToPriceSnapshotDto(this List<PriceSnapshotModel> tickers)
    {
        if (tickers == null)
        {
            return [];
        }
        return tickers.Select(t => t.MapToPriceSnapshotDto()).ToList();
    }
}