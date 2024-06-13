using Microsoft.Extensions.Logging;
using TradingBot.Domain.Mapping;
using TradingBot.Domain.Model;
using TradingBot.Domain.Provider;
using TradingBot.Domain.Repository.Position;
using TradingBot.Domain.Repository.Ticker;
using TradingBot.Domain.Service;

namespace TradingBot.UseCases.Services;

public class CoinSpotExchangeService(
    IPositionRepository positionRepository,
    ITickerRepository tickerRepository,
    IExchangeProvider exchangeProvider,
    ILogger<CoinSpotExchangeService> logger)
    : IExchangeService
{
    private const string Exchange = "CoinSpot";
    private readonly List<string> _tickers = ["BTC", "ETH", "XRP"];

    public async Task<List<PriceSnapshotModel>> GetTickersAsync()
    {
        try
        {
            logger.LogInformation("Getting tickers");
            var tickers = await exchangeProvider.GetTickers();
            if (tickers.Count > 0 && !tickerRepository.SaveTickers(tickers.MapToPriceSnapshotDto()))
            {
                logger.LogError("Failed to save tickers");
                throw new Exception("Failed to save tickers");
            }

            logger.LogInformation("Tickers: {tickers}", tickers);
            return tickers.Where(t => _tickers.Contains(t.Name)).ToList();
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to get tickers");
            throw;
        }
    }

    public async Task<PortfolioModel> GetPortfoliosAsync()
    {
        try
        {
            logger.LogInformation("Getting portfolio");
            var positions = await exchangeProvider.GetPortfolio();
            if (!positionRepository.SavePositions(Exchange, positions.MapToPositionDto()))
            {
                logger.LogError("Failed to save positions");
                throw new Exception("Failed to save positions");
            }
            logger.LogInformation("Portfolio: {positions}", positions);
            return positions.MapToPortfolioModel(Exchange);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to get portfolio");
            throw;
        }
    }

    public async Task<PositionModel> BuyAsync(string tickerName, decimal quantity)
    {
        try
        {
            logger.LogInformation("Buying {quantity} {tickerName}", quantity, tickerName);
            var position = await exchangeProvider.Buy(tickerName, quantity);
            if (!positionRepository.SavePosition(Exchange, new PositionDto(position.Name, position.Quantity)))
            {
                logger.LogError("Failed to save position");
                throw new Exception("Failed to save position");
            }
            logger.LogInformation("Bought {quantity} {tickerName}", quantity, tickerName);
            return position;
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to buy {quantity} {tickerName}", quantity, tickerName);
            throw;
        }
    }

    public async Task<PositionModel> SellAsync(string tickerName, decimal quantity)
    {
        try
        {
            logger.LogInformation("Selling {quantity} {tickerName}", quantity, tickerName);
            var position = await exchangeProvider.Sell(tickerName, quantity);
            if (!positionRepository.SavePosition(Exchange, new PositionDto(position.Name, position.Quantity)))
            {
                logger.LogError("Failed to save position");
                throw new Exception("Failed to save position");
            }
            logger.LogInformation("Sold {quantity} {tickerName}", quantity, tickerName);
            return position;
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to sell {quantity} {tickerName}", quantity, tickerName);
            throw;
        }
    }
}