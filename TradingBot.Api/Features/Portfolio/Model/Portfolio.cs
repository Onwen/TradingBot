using Reinforced.Typings.Attributes;

namespace TradingBot.Api.Features.Profile.Model;
[TsInterface]
public class Portfolio
{
    public string Exchange { get; set; } = string.Empty;
    public List<Position> Positions { get; set; } = [];
    public decimal TotalValue => Positions.Sum(p => p.TotalValue);
}
[TsInterface]
public class Position(DateTimeOffset utcNow)
{
    public string Exchange { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal Quantity { get; set; } = 0m;
    public decimal CurrentPrice { get; set; } = 0m;
    public decimal TotalValue => Quantity * CurrentPrice;
    public DateTimeOffset Timestamp { get; set; } = utcNow;
}