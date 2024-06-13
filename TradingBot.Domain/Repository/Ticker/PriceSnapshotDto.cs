using Microsoft.EntityFrameworkCore;
using TradingBot.Domain.Enum;

namespace TradingBot.Domain.Repository.Ticker;

public record PriceSnapshotDto(string Name, Currency Currency, decimal Bid, decimal Ask, decimal Last)
{
    public int Id { get; init; }
    public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UtcNow;
}