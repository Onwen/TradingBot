using Microsoft.Extensions.Logging;
using TradingBot.Domain.API.CoinSpotAPI;
using TradingBot.Domain.API.CoinSpotAPI.Request;
using TradingBot.Domain.Mapping;
using TradingBot.Domain.Model;
using TradingBot.Domain.Provider;

namespace TradingBot.Infrastructure.Provider;

public class CoinSpotExchangeProvider(ICoinSpotApi apiClient, TimeProvider timeProvider,
    ILogger<CoinSpotExchangeProvider> logger) : IExchangeProvider
{
    private const string Exchange = "CoinSpot";

    public async Task<PriceSnapshotModel> GetTicker(string tickerName)
    {
        try
        {
            logger.LogInformation("Getting ticker: {tickerName}", tickerName);
            var tickers = await GetPriceSnapshots();
            var ticker = tickers.FirstOrDefault(t => t.Name == tickerName);
            if (ticker == null)
            {
                logger.LogError("Failed to get ticker");
                throw new Exception("Failed to get ticker");
            }
            logger.LogInformation("Ticker: {ticker}", ticker);
            return ticker;
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to get ticker");
            throw;
        }
    }

    public async Task<List<PriceSnapshotModel>> GetPriceSnapshots()
    {
        try
        {
            logger.LogInformation("Getting tickers");
            var response = await apiClient.GetLatestPrices(new GetLatestPricesRequest());
            if (response.Status != "ok")
            {
                logger.LogError("Failed to get latest prices");
                throw new Exception("Failed to get latest prices");
            }
            logger.LogInformation("Tickers: {tickers}", response.Prices);
            return response.MapToTickerModels(Exchange);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to get tickers");
            throw;
        }
    }
    
    public async Task<MarketOrderModel> MarketBuy(string tickerName, decimal quantity)
    {
        try
        {
            // get latest prices for that ticker
            var ticker = await GetTicker(tickerName);
            // get current portfolio balance of cash
            var portfolio = await GetPortfolio();
            var cash = portfolio.FirstOrDefault(p => p.Name == "AUD")?.Quantity ?? 0;
            // calculate the total cost of the buy order
            var totalCost = ticker.Ask * quantity;
            // check if there is enough cash in the portfolio
            if (cash < totalCost)
            {
                logger.LogError("Insufficient funds to buy {quantity} {tickerName}", quantity, tickerName);
                throw new Exception("Insufficient funds to buy");
            }
            // place the buy order
            logger.LogInformation("Placing MarketBuy Order {quantity} {tickerName}", quantity, tickerName);
            var response = await apiClient.PlaceMarketBuyOrder(new PlaceMarketBuyOrderRequest()
                { Amount = quantity, CoinType = tickerName, Rate = ticker.Ask, MarketType = "AUD"});
            // confirm the buy order was successful
            if (response.Status == "ok") return response.ToMarketOrderModel(Exchange, timeProvider.GetUtcNow());
            logger.LogError("Failed to place market buy order");
            throw new Exception("Failed to place market buy order");
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to buy {quantity} {tickerName}", quantity, tickerName);
            throw;
        }
    }

    public async Task<MarketOrderModel> MarketSell(string tickerName, decimal quantity)
    {
        try
        {
            // get latest prices for that ticker
            var ticker = await GetTicker(tickerName);
            // get current portfolio balance of ticker
            var portfolio = await GetPortfolio();
            var totalOwned = portfolio.FirstOrDefault(p => p.Name == tickerName)?.Quantity ?? 0;
            if (totalOwned < quantity)
            {
                logger.LogError("Insufficient {tickerName} to sell {quantity}", tickerName, quantity);
                throw new Exception("Insufficient {tickerName} to sell");
            }
            // place the sell order
            logger.LogInformation("Placing MarketSell Order {quantity} {tickerName}", quantity, tickerName);
            var response = await apiClient.PlaceMarketSellOrder(new PlaceMarketSellOrderRequest()
                { Amount = quantity, CoinType = tickerName, Rate = ticker.Bid, MarketType = "AUD"});
            // confirm the sell order was successful
            if (response.Status == "ok") return response.ToMarketOrderModel(Exchange, timeProvider.GetUtcNow());
            logger.LogError("Failed to place market sell order");
            throw new Exception("Failed to place market sell order");
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to sell {quantity} {tickerName}", quantity, tickerName);
            throw;
        }
    }

    public async Task<List<MarketOrderModel?>> GetCompletedMarketOrders()
    {
        try
        {
            logger.LogInformation("Getting market orders");
            // get market orders
            var response = await apiClient.GetCompletedMarketOrders(new GetCompletedMarketOrdersRequest());
            if (response.Status != "ok")
            {
                logger.LogError("Failed to get market orders");
                throw new Exception("Failed to get market orders");
            }
            logger.LogInformation("Market MarketBuy Orders: {orders}", response.BuyOrders);
            logger.LogInformation("Market MarketSell Orders: {orders}", response.SellOrders);
            return response.ToMarketOrderModel(Exchange, timeProvider.GetUtcNow());
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
    
    public async Task<bool> CancelMarketOrder(string orderId, string orderType)
    {
        try
        {
            logger.LogInformation("Cancelling market order {orderId}", orderId);
            var request = new CancelMarketOrderRequest() { OrderId = orderId };
            var response = orderType == "BUY" ? await apiClient.CancelMarketBuyOrder(request) : await apiClient.CancelMarketSellOrder(request);
            if (response.Status == "ok") return true;
            logger.LogError("Failed to cancel market order {orderId}", orderId);
            throw new Exception("Failed to cancel market order");
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to cancel market order {orderId}", orderId);
            throw;
        }
    }

    public async Task<List<PositionModel>> GetPortfolio()
    {
        try
        {
            logger.LogInformation("Getting portfolio");
            var response = await apiClient.GetMyBalances(new GetMyBalancesRequest());
            if (response.Status != "ok")
            {
                logger.LogError("Failed to get my balances");
                throw new Exception("Failed to get my balances");
            }
            
            logger.LogInformation("Portfolio: {positions}", response.Balances);
            return response.MapToPositionModel(Exchange, timeProvider.GetUtcNow());
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to get portfolio");
            throw;
        }
    }
}