using TradingBot.Domain.API.CoinSpotAPI.Response;
using TradingBot.Domain.Model;

namespace TradingBot.Domain.Mapping;

public static class GetMyBalancesResponseMappingExtension
{
    // map GetMyBalancesResponse to PositionModel
    public static List<PositionModel> MapToPositionModel(this GetMyBalancesResponse response)
    {
        if (response == null || response.Balances == null)
        {
            return [];
        }

        List<PositionModel> result = [];
        foreach (var balance in response.Balances)
        {
            foreach (var pair in balance)
            {
                //TODO: improve this ugly code.
                var position = new PositionModel
                {
                    Name = pair.Key,
                    Quantity = pair.Value.Balance
                };
                result.Add(position);
            }
        }

        return result;
    }
}