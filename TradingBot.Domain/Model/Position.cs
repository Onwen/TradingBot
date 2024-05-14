using TradingBot.Domain.Enum;

namespace TradingBot.Domain.Model;

public class Position
{
    private TickerData? _ticker = null;
    public decimal Quantity = 0m;

    public string Name => _ticker?.Name ?? "";
    public decimal Value => (_ticker?.Value ?? 0) * Quantity;
    public Currency Currency => _ticker?.Currency ?? Currency.none;
}