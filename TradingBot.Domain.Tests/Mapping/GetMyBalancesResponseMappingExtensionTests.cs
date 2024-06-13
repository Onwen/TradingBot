using TradingBot.Domain.API.CoinSpotAPI.Response;
using TradingBot.Domain.Mapping;

namespace TradingBot.Domain.Tests;

public class GetMyBalancesResponseMappingExtensionTests
{
    // test mapping GetMyBalancesResponse to List<PositionModel> successfully
    [Fact]
    public void MapToPositionModel_Success()
    {
        // Arrange
        var response = new GetMyBalancesResponse
        {
            Balances = [
            
                new Dictionary<string, BalanceDetail>()
                {
                    {
                        "BTC", new()
                        {
                            Balance = 1
                        }
                    }
                },
                new Dictionary<string, BalanceDetail>()
                {
                    {
                        "ETH", new()
                        {
                            Balance = 2
                        }
                    }
                }
            ]
        };

        // Act
        var result = response.MapToPositionModel();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal("BTC", result[0].Name);
        Assert.Equal(1, result[0].Quantity);
        Assert.Equal("ETH", result[1].Name);
        Assert.Equal(2, result[1].Quantity);
    }
    // test mapping GetMyBalancesResponse to List<PositionModel> with null response
    [Fact]
    public void MapToPositionModel_NullResponse()
    {
        // Arrange
        GetMyBalancesResponse response = null;

        // Act
        var result = response.MapToPositionModel();

        // Assert
        Assert.Empty(result);
    }
    // test mapping GetMyBalancesResponse to List<PositionModel> with null balances
    [Fact]
    public void MapToPositionModel_NullBalances()
    {
        // Arrange
        var response = new GetMyBalancesResponse
        {
            Balances = null
        };

        // Act
        var result = response.MapToPositionModel();

        // Assert
        Assert.Empty(result);
    }
    // test mapping GetMyBalancesResponse to List<PositionModel> with empty balances
    [Fact]
    public void MapToPositionModel_EmptyBalances()
    {
        // Arrange
        var response = new GetMyBalancesResponse
        {
            Balances = []
        };

        // Act
        var result = response.MapToPositionModel();

        // Assert
        Assert.Empty(result);
    }
}