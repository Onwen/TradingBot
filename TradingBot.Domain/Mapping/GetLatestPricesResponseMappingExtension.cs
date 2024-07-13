using TradingBot.Domain.API.CoinSpotAPI.Response;
using TradingBot.Domain.Model;

namespace TradingBot.Domain.Mapping;

public static class GetLatestPricesResponseMappingExtension
{
    // map the response from the API to the ticker model
    public static List<PriceSnapshotModel> MapToTickerModels(this GetLatestPricesResponse response, string exchange)
    {
        if (response == null || response.Prices == null)
        {
            return [];
        }

        var result = new List<PriceSnapshotModel>();
        foreach (var item in response.Prices)
        {
            if (item.Value == null)
            {
                continue;
            }
            var ticker = new PriceSnapshotModel
            {
                Exchange = exchange,
                Name = item.Key,
                Ask = decimal.TryParse(item.Value.Ask, out var askOut) ? askOut : 0,
                Bid = decimal.TryParse(item.Value.Bid, out var bidOut) ? bidOut : 0,
                Last = decimal.TryParse(item.Value.Last, out var lastOut) ? lastOut : 0
            };
            result.Add(ticker);
        }

        return result;
    }
    
}