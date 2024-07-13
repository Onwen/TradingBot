using TradingBot.Domain.Model;
using TradingBot.Domain.Repository.Ticker;

namespace TradingBot.Domain.Mapping;

public static class PriceSnapshotDtoMappingExtension
{
    public static PriceSnapshotModel MapToPriceSnapshotModel(this PriceSnapshotDto dto)
    {
        if (dto == null)
        {
            return null;
        }
        
        return new PriceSnapshotModel
        {
            Exchange = dto.Exchange,
            Name = dto.Name,
            Currency = dto.Currency,
            Bid = dto.Bid,
            Ask = dto.Ask,
            Last = dto.Last,
            Timestamp = dto.Timestamp
        };
    }
    
    public static List<PriceSnapshotModel> MapToPriceSnapshotModel(this List<PriceSnapshotDto> dtos)
    {
        if (dtos == null)
        {
            return [];
        }
        return dtos.Select(dto => dto.MapToPriceSnapshotModel()).ToList();
    }
}