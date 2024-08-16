using Microsoft.Extensions.Logging;
using TradingBot.Domain.Enum;
using TradingBot.Domain.Mapping;
using TradingBot.Domain.Model;
using TradingBot.Domain.Provider;
using TradingBot.Domain.Repository.PriceHistory;
using TradingBot.Domain.Repository.Ticker;
using TradingBot.Domain.Service;

namespace TradingBot.UseCases.Services;

public class PricingService(
    IPositionSnapshotRepository positionSnapshotRepository,
    IPriceHistoryRepository priceHistoryRepository,
    IExchangeProvider exchangeProvider,
    TimeProvider timeProvider,
    ILogger<PricingService> logger)
    :  IPricingService
{
    private readonly List<string> _tickers = [Coin.BTC, Coin.ETH, Coin.XRP, Coin.LTC, Coin.DOGE];
    public async Task<List<PriceSnapshotModel>> GetPriceSnapshotsAsync()
    {
        try
        {
            logger.LogInformation("Getting price snapshots");
            var tickers = await exchangeProvider.GetPriceSnapshots();
            if (tickers.Count > 0 && !positionSnapshotRepository.SavePriceSnapshots(tickers.MapToPriceSnapshotDto(timeProvider.GetUtcNow())))
            {
                logger.LogError("Failed to save price snapshots");
                throw new Exception("Failed to save price snapshots");
            }

            logger.LogInformation("Price snapshots: {tickers}", tickers);
            return tickers.Where(t => _tickers.Contains(t.Name.ToUpper())).ToList();
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