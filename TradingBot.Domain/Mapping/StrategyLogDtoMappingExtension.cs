using TradingBot.Domain.Model;
using TradingBot.Domain.Repository.StrategyLog;

namespace TradingBot.Domain.Mapping;

public static class StrategyLogDtoMappingExtension
{
    // Map a StrategyLogDto to a StrategyLogModel
    public static StrategyLogModel MapToStrategyLogModel(this StrategyLogDto dto)
    {
        if (dto == null)
        {
            return null;
        }
        
        return new StrategyLogModel
        {
            StrategyName = dto.StrategyName,
            Message = dto.Message,
            Timestamp = dto.Timestamp,
            Id = dto.Id
        };
    }
    // Map a List<StrategyLogDto> to a List<StrategyLogModel>
    public static List<StrategyLogModel> MapToStrategyLogModel(this List<StrategyLogDto> dtos)
    {
        if (dtos == null)
        {
            return [];
        }
        return dtos.Select(dto => dto.MapToStrategyLogModel()).ToList();
    }
}