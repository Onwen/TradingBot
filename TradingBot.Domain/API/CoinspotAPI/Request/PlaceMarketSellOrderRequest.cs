using System.Text.Json.Serialization;
using TradingBot.Domain.API.CoinspotAPI.Converter;

namespace TradingBot.Domain.API.CoinSpotAPI.Request;

public class PlaceMarketSellOrderRequest : BaseRequest
{
    [JsonPropertyName("cointype")]
    public string CoinType { get; set; } = string.Empty;

    [JsonPropertyName("amount")]
    [JsonConverter(typeof(DecimalTo8PlacesConverter))]
    public decimal Amount { get; set; }

    [JsonPropertyName("rate")]
    [JsonConverter(typeof(DecimalTo8PlacesConverter))]
    public decimal Rate { get; set; }

    [JsonPropertyName("markettype")]
    public string MarketType { get; set; } = "AUD";
}