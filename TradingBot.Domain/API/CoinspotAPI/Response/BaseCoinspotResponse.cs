using Newtonsoft.Json;

namespace TradingBot.Domain.API.CoinSpotAPI.Response;

public abstract class BaseCoinSpotResponse
{
    [JsonProperty(PropertyName = "status")]
    public string? Status { get; set; }
    [JsonProperty(PropertyName = "message")]
    public string? Message { get; set; }
}