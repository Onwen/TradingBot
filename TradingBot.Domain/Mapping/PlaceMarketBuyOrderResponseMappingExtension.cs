using TradingBot.Domain.API.CoinSpotAPI.Response;
using TradingBot.Domain.Model;

namespace TradingBot.Domain.Mapping;

public static class PlaceMarketBuyOrderResponseMappingExtension
{
    public static MarketOrderModel ToMarketOrderModel(this PlaceMarketBuyOrderResponse response, string exchange, DateTimeOffset utcNow)
    {
        return new MarketOrderModel
        {
            Exchange = exchange,
            OrderType = "BUY",
            Coin = response.Coin,
            Market = response.Market,
            Amount = response.Amount,
            Rate = response.Rate,
            Id = response.Id,
            Timestamp = utcNow
        };
    }
}