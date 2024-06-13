using TradingBot.Domain.Model;

namespace TradingBot.Domain.Service;

// create an IExchangeService interface that abstracts the logic for interacting with multiple IExchangeproviders
// this interface will be implemented by the ExchangeService class
public interface IExchangeService
{
    // define a method that will return a list of PriceSnapshotModel objects
    Task<List<PriceSnapshotModel>> GetTickersAsync();

    // define a method that will return a PortfolioModel objects
    Task<PortfolioModel> GetPortfoliosAsync();
    
    // define a method that will buy a coin on a specific exchange at market price and return a PositionModel object
    Task<PositionModel> BuyAsync(string tickerName, decimal quantity);
    
    // define a method that will sell a coin on a specific exchange at market price and return a PositionModel object
    Task<PositionModel> SellAsync(string tickerName, decimal quantity);
}