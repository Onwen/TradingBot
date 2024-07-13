using TradingBot.Domain.Model;
using TradingBot.Domain.Repository.StrategyLog;

namespace TradingBot.Domain.Mapping;

public static class StrategyLogModelMappingExtension
{
    // Map a StrategyLogModel to a StrategyLogDto
    public static StrategyLogDto MapToStrategyLogDto(this StrategyLogModel model)
    {
        if (model == null)
        {
            return null;
        }

        return new StrategyLogDto
        (
            model.StrategyName,
            model.Message,
            model.Timestamp
        );
    }
    // Map a List<StrategyLogModel> to a List<StrategyLogDto>
    public static List<StrategyLogDto> MapToStrategyLogDto(this List<StrategyLogModel> models)
    {
        if (models == null)
        {
            return [];
        }
        return models.Select(model => model.MapToStrategyLogDto()).ToList();
    }
}