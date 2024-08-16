using TradingBot.Domain.Model;
using TradingBot.Domain.Service;

namespace TradingBot.UseCases.Services.Backtest;

public class BacktestPortfolioService : IPortfolioService
{
    private PortfolioModel _portfolioModel = new();
    public Task<PortfolioModel> GetPortfoliosAsync()
    {
        return Task.FromResult(_portfolioModel);
    }
    
    public Task<bool> SavePortfolioAsync(PortfolioModel portfolio)
    {
        _portfolioModel = portfolio;
        return Task.FromResult(true);
    }
}