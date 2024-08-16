using Microsoft.Extensions.Logging;
using TradingBot.Domain.Enum;
using TradingBot.Domain.Mapping;
using TradingBot.Domain.Model;
using TradingBot.Domain.Repository.PriceHistory;
using TradingBot.Domain.Repository.Ticker;
using TradingBot.Domain.Service;

namespace TradingBot.UseCases.Services.Backtest;

public class BacktestPricingService(
    IPositionSnapshotRepository positionSnapshotRepository,
    IPriceHistoryRepository priceHistoryRepository,
    TimeProvider timeProvider,
    ILogger<PricingService> logger) : IPricingService
{
    private readonly List<string> _tickers = [Coin.BTC, Coin.ETH, Coin.XRP, Coin.LTC, Coin.DOGE];
    public async Task<List<PriceSnapshotModel>> GetPriceSnapshotsAsync()
    {
        try
        {
            logger.LogInformation("Getting price snapshots");
            var tickers = await priceHistoryRepository.GetPriceHistory(timeProvider.GetUtcNow().AddDays(-1).DateTime, timeProvider.GetUtcNow().DateTime);

            logger.LogInformation("Price snapshots: {tickers}", tickers);
            return tickers.ToList().MapToPriceSnapshotModel().Where(t => _tickers.Contains(t.Name.ToUpper())).ToList();
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to get price snapshots");
            throw;
        }
    }
    
    public async Task<List<PriceHistoryModel>> GetDailyPricesAsync(DateTimeOffset from, DateTimeOffset to)
    {
        try
        {
            logger.LogInformation("Getting daily prices");
            var prices = await priceHistoryRepository.GetPriceHistory(from.DateTime, to.DateTime);
            logger.LogInformation("Daily prices: {prices}", prices);
            return prices.ToList().ToModel();
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to get daily prices");
            throw;
        }
    }
}