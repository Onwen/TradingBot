using TradingBot.Domain.Enum;
using TradingBot.Domain.Model;
using TradingBot.Domain.Repository.Ticker;

namespace TradingBot.Domain.Mapping;

public static class PriceSnapshotModelMappingExtension
{
    // map price snapshot model to price snapshot dto
    public static PriceSnapshotDto MapToPriceSnapshotDto(this PriceSnapshotModel priceSnapshot, DateTimeOffset utcNow)
    {
        if (priceSnapshot == null)
        {
            return null;
        }
        return new PriceSnapshotDto(priceSnapshot.Exchange, priceSnapshot.Name, Currency.AUD,priceSnapshot.Bid, priceSnapshot.Ask, priceSnapshot.Last, utcNow);
    }
    // map list of price snapshot model to list of price snapshot dto
    public static List<PriceSnapshotDto> MapToPriceSnapshotDto(this List<PriceSnapshotModel> tickers, DateTimeOffset utcNow)
    {
        if (tickers == null)
        {
            return [];
        }
        return tickers.Select(t => t.MapToPriceSnapshotDto(utcNow)).ToList();
    }
}