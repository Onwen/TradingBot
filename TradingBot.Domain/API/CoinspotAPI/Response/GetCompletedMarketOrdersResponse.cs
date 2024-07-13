namespace TradingBot.Domain.API.CoinSpotAPI.Response;

public class GetCompletedMarketOrdersResponse : BaseCoinSpotResponse
{
    public List<Order> BuyOrders { get; set; }
    public List<Order> SellOrders { get; set; }
}

public class Order
{
    public string Id { get; set; }
    public string Coin { get; set; }
    public string Market { get; set; }
    public decimal Rate { get; set; }
    public decimal Amount { get; set; }
    public decimal Total { get; set; }
    public DateTimeOffset SoldDate { get; set; }
    public decimal AudFeeExGst { get; set; }
    public decimal AudGst { get; set; }
    public decimal AudTotal { get; set; }
}