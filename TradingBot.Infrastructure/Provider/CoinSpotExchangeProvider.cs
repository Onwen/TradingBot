using Microsoft.Extensions.Logging;
using TradingBot.Domain.API.CoinSpotAPI;
using TradingBot.Domain.API.CoinSpotAPI.Request;
using TradingBot.Domain.Mapping;
using TradingBot.Domain.Model;
using TradingBot.Domain.Provider;

namespace TradingBot.Infrastructure.Provider;

public class CoinSpotExchangeProvider(ICoinSpotApi apiClient,
    ILogger<CoinSpotExchangeProvider> logger) : IExchangeProvider
{
    public async Task<PriceSnapshotModel> GetTicker(string tickerName)
    {
        try
        {
            logger.LogInformation("Getting ticker: {tickerName}", tickerName);
            var tickers = await GetTickers();
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

    public async Task<List<PriceSnapshotModel>> GetTickers()
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
            return response.MapToTickerModels();
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to get tickers");
            throw;
        }
    }

    public async Task<PositionModel> Buy(string tickerName, decimal quantity)
    {
        try
        {
            logger.LogInformation("Buying {quantity} {tickerName}", quantity, tickerName);
            var response = await apiClient.PlaceMarketBuyNowOrder(new PlaceMarketBuyNowOrderRequest()
                { Amount = quantity, CoinType = tickerName, AmountType = "coin"});
            if (response.Status != "ok")
            {
                logger.LogError("Failed to place market buy order");
                throw new Exception("Failed to place market buy order");
            }

            logger.LogInformation("Bought {quantity} {tickerName}", quantity, tickerName);
            return response.MapToPositionModel();
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to buy {quantity} {tickerName}", quantity, tickerName);
            throw;
        }
    }

    public async Task<PositionModel> Sell(string tickerName, decimal quantity)
    {
        try{
            logger.LogInformation("Selling {quantity} {tickerName}", quantity, tickerName);
            var response = await apiClient.PlaceMarketSellNowOrder(new PlaceMarketSellNowOrderRequest()
                { Amount = quantity, CoinType = tickerName, AmountType = "coin"});
            if (response.Status != "ok")
            {
                logger.LogError("Failed to place market sell order");
                throw new Exception("Failed to place market sell order");
            }

            logger.LogInformation("Sold {quantity} {tickerName}", quantity, tickerName);
            return response.MapToPositionModel();
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to sell {quantity} {tickerName}", quantity, tickerName);
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
            return response.MapToPositionModel();
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to get portfolio");
            throw;
        }
    }
}