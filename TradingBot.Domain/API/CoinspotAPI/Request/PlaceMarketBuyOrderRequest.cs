using Newtonsoft.Json;

namespace TradingBot.Domain.API.CoinSpotAPI.Request;

public class PlaceMarketBuyOrderRequest : BaseRequest
{
    [JsonProperty(PropertyName = "cointype")]
    public string CoinType { get; set; } = string.Empty;

    [JsonProperty(PropertyName = "amount")]
    public decimal Amount { get; set; }

    [JsonProperty(PropertyName = "rate")] public decimal Rate { get; set; }

    [JsonProperty(PropertyName = "markettype")]
    public string MarketType { get; set; } = "AUD";
}