using TradingBot.Domain.Model;

namespace TradingBot.Domain.Service;

public interface IPortfolioService
{
    // define a method that will return a PortfolioModel objects
    Task<PortfolioModel> GetPortfoliosAsync();
    Task<bool> SavePortfolioAsync(PortfolioModel portfolio);
}