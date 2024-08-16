namespace TradingBot.Domain.Model;

public class PriceHistoryModel
{
    public string Name { get; set; }
    public string Symbol { get; set; }
    public DateTimeOffset TimeOpen { get; set; }
    public decimal Open { get; set; }
    public DateTimeOffset TimeHigh { get; set; }
    public decimal High { get; set; }
    public DateTimeOffset TimeLow { get; set; }
    public decimal Low { get; set; }
    public DateTimeOffset TimeClose { get; set; }
    public decimal Close { get; set; }
    public decimal Volume { get; set; }
    public decimal MarketCap { get; set; }
}