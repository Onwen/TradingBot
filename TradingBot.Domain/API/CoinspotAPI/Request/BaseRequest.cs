namespace TradingBot.Domain.API.CoinSpotAPI.Request;

public class BaseRequest
{
    public static string NoncePlaceholder { get; } = "nonce_placeholder";
    public string Nonce { get; } = NoncePlaceholder;
}