using Microsoft.Extensions.Logging;
using TradingBot.Domain.Mapping;
using TradingBot.Domain.Model;
using TradingBot.Domain.Provider;
using TradingBot.Domain.Repository.Order;
using TradingBot.Domain.Repository.Position;
using TradingBot.Domain.Repository.PositionTargetWeighting;
using TradingBot.Domain.Repository.Return;
using TradingBot.Domain.Repository.StrategyLog;
using TradingBot.Domain.Repository.Ticker;
using TradingBot.Domain.Service;

namespace TradingBot.UseCases.Services;

public class CoinSpotExchangeService(
    IPositionRepository positionRepository,
    IPositionSnapshotRepository positionSnapshotRepository,
    IStrategyLogRepository strategyLogRepository,
    IPositionTargetWeightingRepository positionTargetWeightingRepository,
    IReturnRepository returnRepository,
    IOrderRepository orderRepository,
    IExchangeProvider exchangeProvider,
    TimeProvider timeProvider,
    ILogger<CoinSpotExchangeService> logger)
    : IExchangeService
{
    private const string Exchange = "CoinSpot";
    private readonly List<string> _tickers = ["BTC", "ETH", "XRP"];

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
            return tickers.Where(t => _tickers.Contains(t.Name)).ToList();
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to get price snapshots");
            throw;
        }
    }

    public async Task<PortfolioModel> GetPortfoliosAsync()
    {
        try
        {
            logger.LogInformation("Getting portfolio");
            var positions = await exchangeProvider.GetPortfolio();
            var priceSnapshots = await GetPriceSnapshotsAsync();
            if (!positionRepository.SavePositions(Exchange, positions.MapToPositionDto(timeProvider.GetUtcNow())))
            {
                logger.LogError("Failed to save positions");
                throw new Exception("Failed to save positions");
            }
            logger.LogInformation("Portfolio: {positions}", positions);
            return positions.MapToPortfolioModel(priceSnapshots, Exchange, timeProvider.GetUtcNow());
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to get portfolio");
            throw;
        }
    }
    
    public async Task<List<PriceSnapshotModel>> GetDailyPricesAsync(DateTimeOffset from, DateTimeOffset to)
    {
        try
        {
            logger.LogInformation("Getting daily prices");
            var prices = await positionSnapshotRepository.GetDailyPrices(from, to);
            logger.LogInformation("Daily prices: {prices}", prices);
            return prices.MapToPriceSnapshotModel();
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to get daily prices");
            throw;
        }
    }

    public async Task<MarketOrderModel> MarketBuyAsync(string tickerName, decimal quantity)
    {
        try
        {
            logger.LogInformation("Buying {quantity} {tickerName}", quantity, tickerName);
            var marketOrder = await exchangeProvider.MarketBuy(tickerName, quantity);
            if (!orderRepository.SaveOrder(marketOrder.ToOrderDto()))
            {
                logger.LogError("Failed to save order");
                throw new Exception("Failed to save order");
            }
            // wait for order to complete and cancel if it doesn't
            var completedOrder = await AwaitOrderCompletion(marketOrder.Id);
            if (completedOrder == null)
            {
                await exchangeProvider.CancelMarketOrder(marketOrder.Id, marketOrder.OrderType);
                logger.LogError("Failed to buy {quantity} {tickerName}", quantity, tickerName);

                marketOrder.Cancelled = true;
                if (orderRepository.SaveOrder(marketOrder.ToOrderDto())) return marketOrder;
                logger.LogError("Failed to save order");
                throw new Exception("Failed to save order");
            }
            // save completed order
            if (!orderRepository.SaveOrder(completedOrder.ToOrderDto()))
            {
                logger.LogError("Failed to save order");
                throw new Exception("Failed to save order");
            }
            logger.LogInformation("Bought {quantity} {tickerName}", quantity, tickerName);
            return completedOrder;
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to buy {quantity} {tickerName}", quantity, tickerName);
            throw;
        }
    }

    public async Task<MarketOrderModel> MarketSellAsync(string tickerName, decimal quantity)
    {
        try
        {
            logger.LogInformation("Selling {quantity} {tickerName}", quantity, tickerName);
            var marketOrder = await exchangeProvider.MarketSell(tickerName, quantity);
            if (!orderRepository.SaveOrder(marketOrder.ToOrderDto()))
            {
                logger.LogError("Failed to save order");
                throw new Exception("Failed to save order");
            }
            // wait for order to complete and cancel if it doesn't
            var completedOrder = await AwaitOrderCompletion(marketOrder.Id);
            if (completedOrder == null)
            {
                await exchangeProvider.CancelMarketOrder(marketOrder.Id, marketOrder.OrderType);
                logger.LogError("Failed to sell {quantity} {tickerName}", quantity, tickerName);

                marketOrder.Cancelled = true;
                if (orderRepository.SaveOrder(marketOrder.ToOrderDto())) return marketOrder;
                logger.LogError("Failed to save order");
                throw new Exception("Failed to save order");
            }
            // save completed order
            if (!orderRepository.SaveOrder(completedOrder.ToOrderDto()))
            {
                logger.LogError("Failed to save order");
                throw new Exception("Failed to save order");
            }
            logger.LogInformation("Sold {quantity} {tickerName}", quantity, tickerName);
            return completedOrder;
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to sell {quantity} {tickerName}", quantity, tickerName);
            throw;
        }
    }
    
    private async Task<MarketOrderModel?> AwaitOrderCompletion(string orderId)
    {
        const int maxRetries = 10;
        const int sleepTime = 1000;
        for(var retry = 0; retry < maxRetries; retry++)
        {
            var orders = await exchangeProvider.GetCompletedMarketOrders();
            if (orders.All(b => b?.Id != orderId))
            {
                await Task.Delay(sleepTime);
            }
            else
            {
                return orders.First(b => b?.Id == orderId);
            }
        }
        return null;
    }

    public async Task<List<StrategyLogModel>> GetLogs(DateTimeOffset from, DateTimeOffset to)
    {
        try
        {
            logger.LogInformation("Getting logs");
            var logs = await strategyLogRepository.GetLogs(from, to);
            logger.LogInformation("Logs: {logs}", logs);
            return logs.MapToStrategyLogModel();
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to get logs");
            throw;
        }
    }

    public async Task<List<StrategyLogModel>> GetLogsByStrategy(string strategyName, DateTimeOffset from, DateTimeOffset to)
    {
        try
        {
            logger.LogInformation("Getting logs for {strategyName}", strategyName);
            var logs = await strategyLogRepository.GetLogsByStrategy(strategyName, from, to);
            logger.LogInformation("Logs: {logs}", logs);
            return logs.MapToStrategyLogModel();
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to get logs");
            throw;
        }
    }

    public async Task<bool> SaveLog(StrategyLogModel log)
    {
        try
        {
            logger.LogInformation("Saving log");
            return await strategyLogRepository.SaveLog(new StrategyLogDto(log.StrategyName, log.Message, log.Timestamp));
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to save log");
            throw;
        }
    }

    public async Task<List<PositionTargetWeightingModel>> GetPositionTargetWeightingsAsync()
    {
        try
        {
            logger.LogInformation("Getting position target weightings");
            var positionTargetWeightings = await positionTargetWeightingRepository.GetLatestWeightings();
            logger.LogInformation("Position target weightings: {positionTargetWeightings}", positionTargetWeightings);
            return positionTargetWeightings.MapToPositionTargetWeightingModels(timeProvider.GetUtcNow());
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to get position target weightings");
            throw;
        }
    }

    public async Task<bool> SavePositionTargetWeightingsAsync(List<PositionTargetWeightingModel> positionTargetWeightings)
    {
        try
        {
            logger.LogInformation("Saving position target weightings");
            return await positionTargetWeightingRepository.SaveWeightings(positionTargetWeightings.MapToPositionTargetWeightingDtos());
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to save position target weightings");
            throw;
        }
    }

    public async Task<bool> SaveDailyReturns(Dictionary<string, decimal> previousDayReturns)
    {
        try
        {
            logger.LogInformation("Saving daily returns");
            var dtos = previousDayReturns.Select(b => new ReturnDto(b.Key, Exchange, ReturnType.Daily, timeProvider.GetUtcNow(), b.Value)).ToList() ?? [];
            return await returnRepository.SaveReturns(dtos);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to save daily returns");
            throw;
        }
    }
}