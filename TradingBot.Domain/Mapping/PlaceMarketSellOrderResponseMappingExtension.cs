using TradingBot.Domain.API.CoinSpotAPI.Response;
using TradingBot.Domain.Model;

namespace TradingBot.Domain.Mapping;

public static class PlaceMarketSellOrderResponseMappingExtension
{
    public static MarketOrderModel ToMarketOrderModel(this PlaceMarketSellOrderResponse response, string exchange, DateTimeOffset utcNow)
    {
        return new MarketOrderModel
        {
            Exchange = exchange,
            OrderType = "SELL",
            Coin = response.Coin.ToUpper(),
            Market = response.Market.ToUpper(),
            Amount = response.Amount,
            Rate = response.Rate,
            Id = response.Id,
            Timestamp = utcNow
        };
    }
}