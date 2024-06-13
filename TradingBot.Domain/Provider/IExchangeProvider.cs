using TradingBot.Domain.Model;
using TradingBot.Domain.Repository.Ticker;

namespace TradingBot.Domain.Provider;

public interface IExchangeProvider
{
    // function signature for retrieving the current state of a coin (e.g. price, volume, etc.)
    Task<PriceSnapshotModel> GetTicker(string tickerName);
    Task<List<PriceSnapshotModel>> GetTickers();
    // function signature for placing a market buy order
    Task<PositionModel> Buy(string tickerName, decimal quantity);
    // function signature for placing a market sell order
    Task<PositionModel> Sell(string tickerName, decimal quantity);
    // function signature for retrieving the current state of the portfolio (e.g. balance, etc.)
    Task<List<PositionModel>> GetPortfolio();
    
}