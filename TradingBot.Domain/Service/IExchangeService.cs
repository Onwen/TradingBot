using TradingBot.Domain.Model;

namespace TradingBot.Domain.Service;

// create an IExchangeService interface that abstracts the logic for interacting with multiple IExchangeproviders
// this interface will be implemented by the ExchangeService class
public interface IExchangeService
{
    // define a method that will return a list of PriceSnapshotModel objects
    Task<List<PriceSnapshotModel>> GetPriceSnapshotsAsync();

    // define a method that will return a PortfolioModel objects
    Task<PortfolioModel> GetPortfoliosAsync();
    
    // define a method that will return a list of daily price snapshots
    Task<List<PriceSnapshotModel>> GetDailyPricesAsync(DateTimeOffset from, DateTimeOffset to);
    
    // define a method that will buy a coin on a specific exchange at market price and return a PositionModel object
    Task<MarketOrderModel> MarketBuyAsync(string tickerName, decimal quantity);
    
    // define a method that will sell a coin on a specific exchange at market price and return a PositionModel object
    Task<MarketOrderModel> MarketSellAsync(string tickerName, decimal quantity);
    // define a method that will return a list of strategy logs between two dates
    Task<List<StrategyLogModel>> GetLogs(DateTimeOffset from, DateTimeOffset to);
    // define a method that will return a list of strategy logs for a specific strategy between two dates
    Task<List<StrategyLogModel>> GetLogsByStrategy(string strategyName, DateTimeOffset from, DateTimeOffset to);
    // define a method that will save a strategy log
    Task<bool> SaveLog(StrategyLogModel log);
    // define a method that will return a list of PositionTargetWeightingModel objects
    Task<List<PositionTargetWeightingModel>> GetPositionTargetWeightingsAsync();
    // define a method that will save a list of PositionTargetWeightingModel objects
    Task<bool> SavePositionTargetWeightingsAsync(List<PositionTargetWeightingModel> positionTargetWeightings);

    Task<bool> SaveDailyReturns(Dictionary<string, decimal> previousDayReturns);
}