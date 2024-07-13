using TradingBot.Domain.Enum;

namespace TradingBot.Domain.Model;
public class PriceSnapshotModel
{
    public string Exchange = string.Empty;
    public string Name = string.Empty;
    public Currency Currency;
    public decimal Bid;
    public decimal Ask;
    public decimal Last;
    public DateTimeOffset Timestamp;
}