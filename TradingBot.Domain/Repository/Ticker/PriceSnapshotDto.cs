using Microsoft.EntityFrameworkCore;
using TradingBot.Domain.Enum;

namespace TradingBot.Domain.Repository.Ticker;

public record PriceSnapshotDto(string Exchange, string Name, Currency Currency, decimal Bid, decimal Ask, decimal Last, DateTimeOffset Timestamp)
{
    public int Id { get; init; }
}