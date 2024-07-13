using System.Security.AccessControl;
using TradingBot.Domain.API.CoinSpotAPI.Response;
using TradingBot.Domain.Model;

namespace TradingBot.Domain.Mapping;

public static class GetCompletedMarketOrdersResponseMappingExtension
{
    // add mapping for Order to MarketOrderModel
    public static MarketOrderModel ToMarketOrderModel(this Order order, string exchange, string orderType, DateTimeOffset utcNow)
    {
        return new MarketOrderModel
        {
            Market = order.Market,
            Amount = order.Amount,
            Rate = order.Rate,
            Id = order.Id,
            Exchange = exchange,
            OrderType = orderType,
            Coin = order.Coin,
            Total = order.Total,
            AudGst = order.AudGst,
            AudTotal = order.AudTotal,
            SoldDate = order.SoldDate,
            AudFeeExGst = order.AudFeeExGst,
            Timestamp = utcNow
        };
    }
    // add mapping for GetCompletedMarketOrdersResponse.BuyOrders to MarketOrderModel
    public static List<MarketOrderModel?> ToMarketOrderModel(this List<Order> orders, string exchange, string orderType, DateTimeOffset utcNow)
    {
        return orders.Select(order => order.ToMarketOrderModel(exchange, orderType, utcNow)).ToList();
    }
    
    // add mapping for GetCompletedMarketOrdersResponse to MarketOrderModel
    public static List<MarketOrderModel?> ToMarketOrderModel(this GetCompletedMarketOrdersResponse response, string exchange, DateTimeOffset utcNow)
    {
        var buyOrders = response.BuyOrders.ToMarketOrderModel(exchange, "BUY", utcNow);
        var sellOrders = response.SellOrders.ToMarketOrderModel(exchange, "SELL", utcNow);
        return buyOrders.Concat(sellOrders).ToList();
    }
}