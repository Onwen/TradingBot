using TradingBot.Domain.API.CoinSpotAPI.Response;
using TradingBot.Domain.Mapping;

namespace TradingBot.Domain.Tests;

public class GetLatestPricesResponseMappingExtensionTests
{
    // test mapping GetLatestPricesResponse to PriceSnapshotModel successfully
    [Fact]
    public void MapToTickerModels_Success()
    {
        // Arrange
        var response = new GetLatestPricesResponse
        {
            Prices = new Dictionary<string, PriceDetail>
            {
                {
                    "BTC", new()
                    {
                        Ask = "1000",
                        Bid = "900",
                        Last = "950"
                    }
                },
                {
                    "ETH", new()
                    {
                        Ask = "100",
                        Bid = "90",
                        Last = "95"
                    }
                }
            }
        };

        // Act
        var result = response.MapToTickerModels();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal("BTC", result[0].Name);
        Assert.Equal(1000, result[0].Ask);
        Assert.Equal(900, result[0].Bid);
        Assert.Equal(950, result[0].Last);
        Assert.Equal("ETH", result[1].Name);
        Assert.Equal(100, result[1].Ask);
        Assert.Equal(90, result[1].Bid);
        Assert.Equal(95, result[1].Last);
    }
    // test mapping GetLatestPricesResponse to PriceSnapshotModel with null response
    [Fact]
    public void MapToTickerModels_NullResponse()
    {
        // Arrange
        GetLatestPricesResponse response = null;

        // Act
        var result = response.MapToTickerModels();

        // Assert
        Assert.Empty(result);
    }
    // test mapping GetLatestPricesResponse to PriceSnapshotModel with empty Prices
    [Fact]
    public void MapToTickerModels_EmptyPrices()
    {
        // Arrange
        var response = new GetLatestPricesResponse
        {
            Prices = new Dictionary<string, PriceDetail>()
        };

        // Act
        var result = response.MapToTickerModels();

        // Assert
        Assert.Empty(result);
    }
    // test mapping GetLatestPricesResponse to PriceSnapshotModel with null Prices
    [Fact]
    public void MapToTickerModels_NullPrices()
    {
        // Arrange
        var response = new GetLatestPricesResponse
        {
            Prices = null
        };

        // Act
        var result = response.MapToTickerModels();

        // Assert
        Assert.Empty(result);
    }
}