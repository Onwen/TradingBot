using Microsoft.Extensions.Logging;
using TradingBot.Domain.Repository.Position;
using TradingBot.Infrastructure.Repository.DataContext;

namespace TradingBot.Infrastructure.Repository;

public class PositionRepository(ApplicationDbContext dataContext, ILogger<PositionRepository> logger) : IPositionRepository
{
    public List<PositionDto> GetPositions(string exchange)
    {
        logger.LogInformation("Getting positions");
        return dataContext.Positions.ToList();
    }
    
    public bool SavePositions(string exchange, List<PositionDto> positions)
    {
        logger.LogInformation("Saving positions");
        var success = false;

        using var transaction = dataContext.Database.BeginTransaction();
        dataContext.Positions.AddRange(positions);
        success = dataContext.SaveChanges() > 0;
        if (success)
        {
            logger.LogInformation("Positions saved");
            transaction.Commit();
        }
        else
        {
            logger.LogError("Failed to save positions");
            transaction.Rollback();
        }
        
        return success;
    }
    
    public bool SavePosition(string exchange, PositionDto position)
    {
        logger.LogInformation("Saving position");
        var success = false;

        using var transaction = dataContext.Database.BeginTransaction();
        dataContext.Positions.Add(position);
        success = dataContext.SaveChanges() > 0;
        if (success)
        {
            logger.LogInformation("Positions saved");
            transaction.Commit();
        }
        else
        {
            logger.LogError("Failed to save positions");
            transaction.Rollback();
        }

        return success;
    }
}