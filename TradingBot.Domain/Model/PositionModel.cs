using TradingBot.Domain.Enum;
using TradingBot.Domain.Repository.Ticker;

namespace TradingBot.Domain.Model;

public class PositionModel
{
    public string Name = string.Empty;
    public decimal Quantity = 0m;
}