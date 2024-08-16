using TradingBot.Domain.Model;
using TradingBot.Domain.Repository.Ticker;

namespace TradingBot.Domain.Service;

public interface IPricingService
{
    // define a method that will return a list of PriceSnapshotModel objects
    Task<List<PriceSnapshotModel>> GetPriceSnapshotsAsync();
    
    // define a method that will return a list of daily price snapshots
    Task<List<PriceHistoryModel>> GetDailyPricesAsync(DateTimeOffset from, DateTimeOffset to);
}