using Newtonsoft.Json;

namespace TradingBot.Domain.API.CoinSpotAPI.Request;

public class PlaceMarketBuyNowOrderRequest : BaseRequest
{
    [JsonProperty(PropertyName = "cointype")]
    public string CoinType { get; set; } = string.Empty;

    [JsonProperty(PropertyName = "amounttype")]
    public string AmountType { get; set; }

    [JsonProperty(PropertyName = "amount")]
    public decimal Amount { get; set; }

    [JsonProperty(PropertyName = "rate")] public decimal Rate { get; set; }

    [JsonProperty(PropertyName = "threshold")]
    public decimal threshold { get; set; } = 0;
}