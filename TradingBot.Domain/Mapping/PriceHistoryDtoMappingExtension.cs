using TradingBot.Domain.Enum;
using TradingBot.Domain.Model;
using TradingBot.Domain.Repository.PriceHistory;

namespace TradingBot.Domain.Mapping;

public static class PriceHistoryDtoMappingExtension
{
    // define a method that maps a PriceHistoryDto object to a PriceHistoryModel object
    public static PriceHistoryModel ToModel(this PriceHistoryDto dto)
    {
        return new PriceHistoryModel
        {
            Name = dto.Name,
            Symbol = dto.Symbol,
            TimeOpen = dto.TimeOpen,
            Open = dto.Open,
            TimeHigh = dto.TimeHigh,
            High = dto.High,
            TimeLow = dto.TimeLow,
            Low = dto.Low,
            TimeClose = dto.TimeClose,
            Close = dto.Close,
            Volume = dto.Volume,
            MarketCap = dto.MarketCap
        };
    }
    //define method that maps a List<PriceHistoryDto> object to a List<PriceHistoryModel> object
    public static List<PriceHistoryModel> ToModel(this List<PriceHistoryDto> dtos)
    {
        return dtos.Select(dto => dto.ToModel()).ToList();
    }
    
    public static PriceSnapshotModel ToPriceSnapshotModel(this PriceHistoryDto dto)
    {
        return new PriceSnapshotModel
        {
            Name = dto.Symbol,
            Timestamp = dto.TimeClose,
            Ask = dto.Close,
            Bid = dto.Close,
            Last = dto.Close,
            Exchange = "",
            Currency = Currency.AUD
        };
    }
    
    public static List<PriceSnapshotModel> MapToPriceSnapshotModel(this List<PriceHistoryDto> dtos)
    {
        return dtos.Select(dto => dto.ToPriceSnapshotModel()).ToList();
    }
}