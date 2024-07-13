using Microsoft.Extensions.Logging;
using TradingBot.Domain.Repository.Order;
using TradingBot.Domain.Repository.Position;
using TradingBot.Infrastructure.Repository.DataContext;

namespace TradingBot.Infrastructure.Repository;

public class OrderRepository(ApplicationDbContext dataContext, ILogger<OrderRepository> logger) : IOrderRepository
{
    public List<OrderDto> GetOrders(string exchange)
    {
        logger.LogInformation("Getting positions");
        return dataContext.Orders.Where(b => b.Exchange == exchange).ToList();
    }
    
    public bool SaveOrders(List<OrderDto> orders)
    {
        logger.LogInformation("Saving orders");
        var success = false;

        using var transaction = dataContext.Database.BeginTransaction();
        dataContext.Orders.AddRange(orders);
        success = dataContext.SaveChanges() > 0;
        if (success)
        {
            logger.LogInformation("Orders saved");
            transaction.Commit();
        }
        else
        {
            logger.LogError("Failed to save orders");
            transaction.Rollback();
        }
        
        return success;
    }
    
    public bool SaveOrder(OrderDto order)
    {
        logger.LogInformation("Saving order");
        var success = false;

        using var transaction = dataContext.Database.BeginTransaction();
        dataContext.Orders.Add(order);
        success = dataContext.SaveChanges() > 0;
        if (success)
        {
            logger.LogInformation("Order saved");
            transaction.Commit();
        }
        else
        {
            logger.LogError("Failed to save order");
            transaction.Rollback();
        }

        return success;
    }
}