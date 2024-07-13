namespace TradingBot.Domain.API.CoinSpotAPI.Request;

public class CancelMarketOrderRequest : BaseRequest
{
    public string OrderId { get; set; } = string.Empty;
}