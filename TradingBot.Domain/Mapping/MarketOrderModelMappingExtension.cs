using TradingBot.Domain.Model;
using TradingBot.Domain.Repository.Order;

namespace TradingBot.Domain.Mapping;

public static class MarketOrderModelMappingExtension
{
    // map from MarketOrder to OrderDto
    public static OrderDto ToOrderDto(this MarketOrderModel marketOrder)
    {
        return new OrderDto(
            marketOrder.Id,
            marketOrder.Exchange,
            marketOrder.OrderType,
            marketOrder.Coin,
            marketOrder.Market,
            marketOrder.Rate,
            marketOrder.Amount,
            marketOrder.Cancelled,
            marketOrder.Total,
            marketOrder.SoldDate,
            marketOrder.AudFeeExGst,
            marketOrder.AudGst,
            marketOrder.AudTotal,
            marketOrder.Timestamp);
    }
}