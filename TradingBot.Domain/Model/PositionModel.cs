using TradingBot.Domain.Enum;
using TradingBot.Domain.Repository.Ticker;

namespace TradingBot.Domain.Model;

public class PositionModel(DateTimeOffset utcNow)
{
    public string Exchange = string.Empty;
    public string Name = string.Empty;
    public decimal Quantity = 0m;
    public decimal CurrentPrice = 0m;
    public decimal TotalValue => Quantity * CurrentPrice;
    public DateTimeOffset Timestamp = utcNow;
}