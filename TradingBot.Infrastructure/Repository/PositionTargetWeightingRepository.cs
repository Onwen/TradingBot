using Microsoft.Extensions.Logging;
using TradingBot.Domain.Repository.PositionTargetWeighting;
using TradingBot.Infrastructure.Repository.DataContext;

namespace TradingBot.Infrastructure.Repository;

public class PositionTargetWeightingRepository(ApplicationDbContext dataContext, ILogger<PositionTargetWeightingRepository> logger) : IPositionTargetWeightingRepository
{
    public async Task<List<PositionTargetWeightingDto>> GetLatestWeightings()
    {
        logger.LogInformation("Getting latest weightings");
        return dataContext.PositionTargetWeightings.ToList();
    }

    public async Task<bool> SaveWeighting(PositionTargetWeightingDto dto)
    {
        logger.LogInformation("Saving weighting");
        var success = false;

        using var transaction = dataContext.Database.BeginTransaction();
        dataContext.PositionTargetWeightings.Add(dto);
        success = dataContext.SaveChanges() > 0;
        if (success)
        {
            logger.LogInformation("Weighting saved");
            transaction.Commit();
        }
        else
        {
            logger.LogError("Failed to save weighting");
            transaction.Rollback();
        }

        return success;
    }

    public async Task<bool> SaveWeightings(List<PositionTargetWeightingDto> dtos)
    {
        logger.LogInformation("Saving weightings");
        var success = false;

        using var transaction = dataContext.Database.BeginTransaction();
        dataContext.PositionTargetWeightings.AddRange(dtos);
        success = dataContext.SaveChanges() > 0;
        if (success)
        {
            logger.LogInformation("Weightings saved");
            transaction.Commit();
        }
        else
        {
            logger.LogError("Failed to save weightings");
            transaction.Rollback();
        }

        return success;
    }
}