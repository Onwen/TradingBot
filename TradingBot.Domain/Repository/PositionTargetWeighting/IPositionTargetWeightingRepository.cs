namespace TradingBot.Domain.Repository.PositionTargetWeighting;

public interface IPositionTargetWeightingRepository
{
    Task<List<PositionTargetWeightingDto>> GetLatestWeightings();
    Task<bool> SaveWeighting(PositionTargetWeightingDto dto);
    Task<bool> SaveWeightings(List<PositionTargetWeightingDto> dtos);
}