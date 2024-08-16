namespace TradingBot.Domain.Repository.PositionTargetWeighting;

public record PositionTargetWeightingDto(string Exchange, string Name, decimal TargetWeighting, DateTime Timestamp)
{
    public int Id { get; init; }
}