using TradingBot.Domain.API.CoinSpotAPI.Response;
using TradingBot.Domain.Mapping;

namespace TradingBot.Domain.Tests;

public class PlaceMarketSellNowOrderResponseMappingExtensionTests
{
    // Test the mapping of the response to the position model
    [Fact]
    public void MapToPositionModel_Success()
    {
        // Arrange
        var response = new PlaceMarketSellNowOrderResponse
        {
            Coin = "BTC",
            Amount = 1
        };

        // Act
        var result = response.MapToPositionModel();

        // Assert
        Assert.Equal("BTC", result.Name);
        Assert.Equal(1, result.Quantity);
    }
    // Test the mapping of the response to the position model with null response
    [Fact]
    public void MapToPositionModel_NullResponse()
    {
        // Arrange
        PlaceMarketSellNowOrderResponse response = null;

        // Act
        var result = response.MapToPositionModel();

        // Assert
        Assert.Null(result);
    }
}