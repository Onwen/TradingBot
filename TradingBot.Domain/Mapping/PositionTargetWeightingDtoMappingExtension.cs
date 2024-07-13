using TradingBot.Domain.Model;
using TradingBot.Domain.Repository.PositionTargetWeighting;

namespace TradingBot.Domain.Mapping;

public static class PositionTargetWeightingDtoMappingExtension
{
    // map from PositionTargetWeightingDto to PositionTargetWeightingModel
    public static PositionTargetWeightingModel MapToPositionTargetWeightingModel(this PositionTargetWeightingDto dto, DateTimeOffset utcNow)
    {
        if (dto == null)
        {
            return null;
        }
        return new PositionTargetWeightingModel(utcNow)
        {
            Exchange = dto.Exchange,
            Name = dto.Name,
            TargetWeighting = dto.TargetWeighting,
            Timestamp = dto.Timestamp
        };
    }
    // map from List<PositionTargetWeightingDto> to List<PositionTargetWeightingModel>
    public static List<PositionTargetWeightingModel> MapToPositionTargetWeightingModels(this List<PositionTargetWeightingDto> dtos, DateTimeOffset utcNow)
    {
        if (dtos == null)
        {
            return [];
        }
        return dtos.Select(dto => dto.MapToPositionTargetWeightingModel(utcNow)).ToList();
    }
}