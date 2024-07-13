using TradingBot.Domain.Model;

namespace TradingBot.Domain.Provider;

public interface IExchangeProvider
{
    // function signature for retrieving the current state of a coin (e.g. price, volume, etc.)
    Task<PriceSnapshotModel> GetTicker(string tickerName);
    Task<List<PriceSnapshotModel>> GetPriceSnapshots();
    // function signature for placing a market buy order
    Task<MarketOrderModel> MarketBuy(string tickerName, decimal quantity);
    // function signature for placing a market sell order
    Task<MarketOrderModel> MarketSell(string tickerName, decimal quantity);
    // function signature for waiting for an order to complete with a timeout
    Task<List<MarketOrderModel?>> GetCompletedMarketOrders();
    // function signature for cancelling an order
    Task<bool> CancelMarketOrder(string orderId, string orderType);
    
    
    // function signature for retrieving the current state of the portfolio (e.g. balance, etc.)
    Task<List<PositionModel>> GetPortfolio();
    
}