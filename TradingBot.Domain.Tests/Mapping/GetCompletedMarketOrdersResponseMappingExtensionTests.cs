using TradingBot.Domain.API.CoinSpotAPI.Response;
using TradingBot.Domain.Mapping;

namespace TradingBot.Domain.Tests.Mapping;

public class GetCompletedMarketOrdersResponseMappingExtensionTests
{
    // test mapping from GetCompletedMarketOrdersResponse to marketOrderModels
    [Fact]
    public void MapToMarketOrderModels()
    {
        // Arrange
        var response = new GetCompletedMarketOrdersResponse
        {
            BuyOrders = new List<Order>
            {
                new Order
                {
                    Id = "1",
                    Coin = "BTC",
                    Market = "AUD",
                    Rate = 1000,
                    Amount = 1,
                    Total = 1000,
                    SoldDate = DateTimeOffset.Now,
                    AudFeeExGst = 10,
                    AudGst = 1,
                    AudTotal = 1011
                }
            },
            SellOrders = new List<Order>
            {
                new Order
                {
                    Id = "2",
                    Coin = "BTC",
                    Market = "AUD",
                    Rate = 1000,
                    Amount = 1,
                    Total = 1000,
                    SoldDate = DateTimeOffset.Now,
                    AudFeeExGst = 10,
                    AudGst = 1,
                    AudTotal = 1011
                }
            }
        };
        var exchange = "exchange";
        var now = DateTimeOffset.UtcNow;

        // Act
        var result = response.ToMarketOrderModel(exchange, now);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.NotNull(result[0]);
        Assert.Equal("exchange", result[0].Exchange);
        Assert.Equal("BUY", result[0].OrderType);
        Assert.Equal("BTC", result[0].Coin);
        Assert.Equal("AUD", result[0].Market);
        Assert.Equal(1, result[0].Amount);
        Assert.Equal(1000, result[0].Rate);
        Assert.Equal("1", result[0].Id);
        Assert.Equal(now, result[0].Timestamp);
        Assert.NotNull(result[1]);
        Assert.Equal("exchange", result[1].Exchange);
        Assert.Equal("SELL", result[1].OrderType);
        Assert.Equal("BTC", result[1].Coin);
        Assert.Equal("AUD", result[1].Market);
        Assert.Equal(1, result[1].Amount);
        Assert.Equal(1000, result[1].Rate);
        Assert.Equal("2", result[1].Id);
        Assert.Equal(now, result[1].Timestamp);
    }
}