namespace TradingBot.Domain.Model;

public class MarketOrderModel
{
    public string Id { get; set; } = string.Empty;
    public string Exchange { get; set; } = string.Empty;
    public string OrderType { get; set; } = string.Empty;
    public string Coin { get; set; } = string.Empty;
    public string Market { get; set; } = string.Empty;
    public decimal Rate { get; set; }
    public decimal Amount { get; set; }
    public bool Cancelled { get; set; }
    public decimal? Total { get; set; }
    public DateTimeOffset? SoldDate { get; set; }
    public decimal? AudFeeExGst { get; set; }
    public decimal? AudGst { get; set; }
    public decimal? AudTotal { get; set; }
    public string Status => SoldDate != null ? "COMPLETED" : "PENDING";
    public DateTimeOffset Timestamp { get; set; }
}