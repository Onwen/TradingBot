namespace TradingBot.Domain.Repository.Order;

public interface IOrderRepository
{
    List<OrderDto> GetOrders(string exchange);

    bool SaveOrders(List<OrderDto> orders);
    bool SaveOrder(OrderDto position);
}