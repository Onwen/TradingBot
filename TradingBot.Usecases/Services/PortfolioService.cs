using Microsoft.Extensions.Logging;
using TradingBot.Domain.Mapping;
using TradingBot.Domain.Model;
using TradingBot.Domain.Provider;
using TradingBot.Domain.Repository.Position;
using TradingBot.Domain.Service;

namespace TradingBot.UseCases.Services;

public class PortfolioService(
    IPositionRepository positionRepository,
    IExchangeProvider exchangeProvider,
    TimeProvider timeProvider,
    ILogger<PortfolioService> logger) : IPortfolioService
{
    private const string Exchange = "CoinSpot";
    public async Task<PortfolioModel> GetPortfoliosAsync()
    {
        try
        {
            logger.LogInformation("Getting portfolio");
            var positions = await exchangeProvider.GetPortfolio();
            var priceSnapshots = await exchangeProvider.GetPriceSnapshots();
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

    public Task<bool> SavePortfolioAsync(PortfolioModel portfolio)
    {
        try
        {
            logger.LogInformation("Saving portfolio");
            if (!positionRepository.SavePositions(Exchange, portfolio.Positions.MapToPositionDto(timeProvider.GetUtcNow())))
            {
                logger.LogError("Failed to save positions");
                throw new Exception("Failed to save positions");
            }
            return Task.FromResult(true);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to save portfolio");
            throw;
        }
    }
}