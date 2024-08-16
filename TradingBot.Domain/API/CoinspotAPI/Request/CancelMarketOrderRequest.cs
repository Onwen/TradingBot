using System.Text.Json.Serialization;

namespace TradingBot.Domain.API.CoinSpotAPI.Request;

public class CancelMarketOrderRequest : BaseRequest
{
    [JsonPropertyName("id")]
    public string OrderId { get; set; } = string.Empty;
}