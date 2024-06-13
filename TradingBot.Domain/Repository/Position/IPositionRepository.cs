namespace TradingBot.Domain.Repository.Position;

public interface IPositionRepository
{
    List<PositionDto> GetPositions(string exchange);

    bool SavePositions(string exchange, List<PositionDto> positions);
    bool SavePosition(string exchange, PositionDto position);
}