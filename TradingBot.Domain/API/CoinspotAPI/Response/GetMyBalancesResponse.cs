using Newtonsoft.Json;

namespace TradingBot.Domain.API.CoinSpotAPI.Response;

public class GetMyBalancesResponse : BaseCoinSpotResponse
{
    [JsonProperty(PropertyName = "balances")]
    public List<Dictionary<string, BalanceDetail>> Balances { get; set; }
}

public class BalanceDetail
{
    public BalanceDetail()
    {
    }

    public BalanceDetail(decimal balance)
    {
        Balance = balance;
    }

    [JsonProperty(PropertyName = "balance")]
    public decimal Balance { get; set; }
    [JsonProperty(PropertyName = "audbalance")]
    public decimal AudBalance { get; set; }
    [JsonProperty(PropertyName = "rate")]
    public decimal Rate { get; set; }
}