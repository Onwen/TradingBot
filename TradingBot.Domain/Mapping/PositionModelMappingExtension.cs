using TradingBot.Domain.Model;
using TradingBot.Domain.Repository.Position;

namespace TradingBot.Domain.Mapping;

public static class PositionModelMappingExtension
{
    // map position model to position dto
    public static PositionDto MapToPositionDto(this PositionModel position, DateTimeOffset utcNow)
    {
        return position == null ? null : new PositionDto(position.Exchange, position.Name, position.Quantity, utcNow);
    }
    // map list of position model to list of position dto
    public static List<PositionDto> MapToPositionDto(this List<PositionModel> positions, DateTimeOffset utcNow)
    {
        if (positions == null)
        {
            return [];
        }
        return positions.Select(p => p.MapToPositionDto(utcNow)).ToList();
    }
    // map list of position model to portfolio model
    public static PortfolioModel MapToPortfolioModel(this List<PositionModel> positions, List<PriceSnapshotModel> priceSnapshots, string exchange, DateTimeOffset utcNow)
    {
        return new PortfolioModel
        {
            Exchange = exchange,
            Positions = positions.Select(b => new PositionModel(utcNow)
            {
                Name = b.Name,
                Quantity = b.Quantity,
                CurrentPrice = priceSnapshots.FirstOrDefault(p => p.Name == b.Name)?.Last ?? 0
            }).ToList()
        };
    }
}