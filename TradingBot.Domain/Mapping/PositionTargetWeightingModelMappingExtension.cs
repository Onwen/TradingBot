using TradingBot.Domain.Model;
using TradingBot.Domain.Repository.PositionTargetWeighting;

namespace TradingBot.Domain.Mapping;

public static class PositionTargetWeightingModelMappingExtension
{
    // map from PositionTargetWeightingModel to PositionTargetWeightingDto
    public static PositionTargetWeightingDto MapToPositionTargetWeightingDto(this PositionTargetWeightingModel model)
    {
        if (model == null)
        {
            return null;
        }

        return new PositionTargetWeightingDto
        (
            model.Exchange,
            model.Name,
            model.TargetWeighting,
            model.Timestamp
        );
    }
    
    // map from List<PositionTargetWeightingModel> to List<PositionTargetWeightingDto>
    public static List<PositionTargetWeightingDto> MapToPositionTargetWeightingDtos(this List<PositionTargetWeightingModel> models)
    {
        if (models == null)
        {
            return [];
        }

        return models.Select(model => model.MapToPositionTargetWeightingDto()).ToList();
    }
}