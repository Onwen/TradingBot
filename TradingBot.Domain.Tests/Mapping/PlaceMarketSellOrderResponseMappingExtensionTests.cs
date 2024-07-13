using TradingBot.Domain.API.CoinSpotAPI.Response;
using TradingBot.Domain.Mapping;

namespace TradingBot.Domain.Tests.Mapping;

public class PlaceMarketSellOrderResponseMappingExtensionTests
{
    // Test the mapping of the response to the position model
    [Fact]
    public void MapToPositionModel_Success()
    {
        // Arrange
        var response = new PlaceMarketSellOrderResponse
        {
            Coin = "BTC",
            Amount = 1,
            Rate = 1,
            Market = "AUD",
            Id = "id"
        };
        const string exchange = "exchange";
        var now = DateTimeOffset.UtcNow;

        // Act
        var result = response.ToMarketOrderModel(exchange, now);

        // Assert
        Assert.Equal("exchange", result.Exchange);
        Assert.Equal("SELL", result.OrderType);
        Assert.Equal("BTC", result.Coin);
        Assert.Equal("AUD", result.Market);
        Assert.Equal(1, result.Amount);
        Assert.Equal(1, result.Rate);
        Assert.Equal(response.Id, result.Id);
        Assert.Equal(now, result.Timestamp);
    }
}