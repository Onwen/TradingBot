using Newtonsoft.Json;

namespace TradingBot.Domain.API.CoinSpotAPI.Response;

public class GetLatestPricesResponse : BaseCoinSpotResponse
{
    [JsonProperty(PropertyName = "prices")]
    public Dictionary<string, PriceDetail> Prices { get; set; }
}

public class PriceDetail
{
    [JsonProperty("bid")]
    public string Bid { get; set; }

    [JsonProperty("ask")]
    public string Ask { get; set; }

    [JsonProperty("last")]
    public string Last { get; set; }
}