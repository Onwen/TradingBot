using TradingBot.Domain.API.CoinSpotAPI.Response;
using TradingBot.Domain.Mapping;
using TradingBot.Domain.Model;
using TradingBot.Domain.Repository.Position;

namespace TradingBot.Domain.Tests;

public class PositionModelMappingExtensionTests
{
    // Test the mapping of position model to position dto
    [Fact]
    public void MapToPositionDto_Success()
    {
        // Arrange
        var positionModel = new PositionModel
        {
            Name = "BTC",
            Quantity = 1
        };

        // Act
        var result = positionModel.MapToPositionDto();

        // Assert
        Assert.Equal("BTC", result.Ticker);
        Assert.Equal(1, result.Quantity);
    }
    // Test the mapping of position model to position dto with null position model
    [Fact]
    public void MapToPositionDto_NullPositionModel()
    {
        // Arrange
        PositionModel positionModel = null;

        // Act
        var result = positionModel.MapToPositionDto();

        // Assert
        Assert.Null(result);
    }
    // Test the mapping of List<PositionModel> to List<PositionDto>
    [Fact]
    public void MapToPositionDto_List_Success()
    {
        // Arrange
        var positionModelList = new List<PositionModel>
        {
            new PositionModel
            {
                Name = "BTC",
                Quantity = 1
            },
            new PositionModel
            {
                Name = "ETH",
                Quantity = 2
            }
        };

        // Act
        var result = positionModelList.MapToPositionDto();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal("BTC", result[0].Ticker);
        Assert.Equal(1, result[0].Quantity);
        Assert.Equal("ETH", result[1].Ticker);
        Assert.Equal(2, result[1].Quantity);
    }
    // Test the mapping of List<PositionModel> to List<PositionDto> with null position model list
    [Fact]
    public void MapToPositionDto_List_NullPositionModelList()
    {
        // Arrange
        List<PositionModel> positionModelList = null;

        // Act
        var result = positionModelList.MapToPositionDto();

        // Assert
        Assert.Empty(result);
    }
    // Test the mapping of List<PositionModel> to List<PositionDto> with empty position model list
    [Fact]
    public void MapToPositionDto_List_EmptyPositionModelList()
    {
        // Arrange
        var positionModelList = new List<PositionModel>();

        // Act
        var result = positionModelList.MapToPositionDto();

        // Assert
        Assert.Empty(result);
    }
    // Test the mapping of List<PositionModel> to PortfolioModel
    [Fact]
    public void MapToPortfolioModel_Success()
    {
        // Arrange
        const string exchange = "TestExchange";
        var positionModelList = new List<PositionModel>
        {
            new PositionModel
            {
                Name = "BTC",
                Quantity = 1
            },
            new PositionModel
            {
                Name = "ETH",
                Quantity = 2
            }
        };

        // Act
        var result = positionModelList.MapToPortfolioModel(exchange);

        // Assert
        Assert.Equal(2, result.Positions.Count);
        Assert.Equal("BTC", result.Positions[0].Name);
        Assert.Equal(1, result.Positions[0].Quantity);
        Assert.Equal("ETH", result.Positions[1].Name);
        Assert.Equal(2, result.Positions[1].Quantity);
    }
}