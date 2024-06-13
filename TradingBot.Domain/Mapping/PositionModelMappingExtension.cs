using TradingBot.Domain.Model;
using TradingBot.Domain.Repository.Position;

namespace TradingBot.Domain.Mapping;

public static class PositionModelMappingExtension
{
    // map position model to position dto
    public static PositionDto MapToPositionDto(this PositionModel position)
    {
        if (position == null)
        {
            return null;
        }
        return new PositionDto(position.Name, position.Quantity);
    }
    // map list of position model to list of position dto
    public static List<PositionDto> MapToPositionDto(this List<PositionModel> positions)
    {
        if (positions == null)
        {
            return [];
        }
        return positions.Select(p => p.MapToPositionDto()).ToList();
    }
    // map list of position model to portfolio model
    public static PortfolioModel MapToPortfolioModel(this List<PositionModel> positions, string exchange)
    {
        return new PortfolioModel
        {
            Exchange = exchange,
            Positions = positions
        };
    }
}