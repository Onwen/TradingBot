using TradingBot.Domain.API.CoinSpotAPI.Response;
using TradingBot.Domain.Model;

namespace TradingBot.Domain.Mapping;

public static class PlaceMarketBuyNowOrderResponseMappingExtension
{
    // map the response from the API to the position model
    public static PositionModel MapToPositionModel(this PlaceMarketBuyNowOrderResponse response)
    {
        if (response == null)
        {
            return null;
        }

        var result = new PositionModel
        {
            Name = response.Coin,
            Quantity = response.Amount
        };

        return result;
    }
}