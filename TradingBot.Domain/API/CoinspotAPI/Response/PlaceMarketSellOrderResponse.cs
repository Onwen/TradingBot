using Newtonsoft.Json;

namespace TradingBot.Domain.API.CoinSpotAPI.Response;

public class PlaceMarketSellOrderResponse : BaseCoinSpotResponse
{
    [JsonProperty(PropertyName = "coin")] public string Coin { get; set; } = string.Empty;

    [JsonProperty(PropertyName = "market")]
    public string Market { get; set; } = string.Empty;

    [JsonProperty(PropertyName = "amount")]
    public decimal Amount { get; set; }

    [JsonProperty(PropertyName = "rate")] public decimal Rate { get; set; }
    [JsonProperty(PropertyName = "id")] public string Id { get; set; }
}