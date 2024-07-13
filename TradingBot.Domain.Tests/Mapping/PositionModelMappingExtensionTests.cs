using TradingBot.Domain.Mapping;
using TradingBot.Domain.Model;

namespace TradingBot.Domain.Tests.Mapping;

public class PositionModelMappingExtensionTests
{
    // Test the mapping of position model to position dto
    [Fact]
    public void MapToPositionDto_Success()
    {
        // Arrange
        var now = DateTimeOffset.UtcNow;
        var positionModel = new PositionModel(now)
        {
            Name = "BTC",
            Quantity = 1
        };

        // Act
        var result = positionModel.MapToPositionDto(now);

        // Assert
        Assert.Equal("BTC", result.Ticker);
        Assert.Equal(1, result.Quantity);
        Assert.Equal(now, result.Timestamp);
    }
    // Test the mapping of position model to position dto with null position model
    [Fact]
    public void MapToPositionDto_NullPositionModel()
    {
        // Arrange
        PositionModel positionModel = null;
        var now = DateTimeOffset.UtcNow;

        // Act
        var result = positionModel.MapToPositionDto(now);

        // Assert
        Assert.Null(result);
    }
    // Test the mapping of List<PositionModel> to List<PositionDto>
    [Fact]
    public void MapToPositionDto_List_Success()
    {
        // Arrange
        var now = DateTimeOffset.UtcNow;
        var positionModelList = new List<PositionModel>
        {
            new PositionModel(now)
            {
                Name = "BTC",
                Quantity = 1
            },
            new PositionModel(now)
            {
                Name = "ETH",
                Quantity = 2
            }
        };

        // Act
        var result = positionModelList.MapToPositionDto(now);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal("BTC", result[0].Ticker);
        Assert.Equal(1, result[0].Quantity);
        Assert.Equal(now, result[0].Timestamp);
        Assert.Equal("ETH", result[1].Ticker);
        Assert.Equal(2, result[1].Quantity);
        Assert.Equal(now, result[1].Timestamp);
    }
    // Test the mapping of List<PositionModel> to List<PositionDto> with null position model list
    [Fact]
    public void MapToPositionDto_List_NullPositionModelList()
    {
        // Arrange
        List<PositionModel> positionModelList = null;
        var now = DateTimeOffset.UtcNow;

        // Act
        var result = positionModelList.MapToPositionDto(now);

        // Assert
        Assert.Empty(result);
    }
    // Test the mapping of List<PositionModel> to List<PositionDto> with empty position model list
    [Fact]
    public void MapToPositionDto_List_EmptyPositionModelList()
    {
        // Arrange
        var positionModelList = new List<PositionModel>();
        var now = DateTimeOffset.UtcNow;

        // Act
        var result = positionModelList.MapToPositionDto(now);

        // Assert
        Assert.Empty(result);
    }
    // Test the mapping of List<PositionModel> to PortfolioModel
    [Fact]
    public void MapToPortfolioModel_Success()
    {
        // Arrange
        var now = DateTimeOffset.UtcNow;
        var positionModelList = new List<PositionModel>
        {
            new PositionModel(now)
            {
                Name = "BTC",
                Quantity = 1
            },
            new PositionModel(now)
            {
                Name = "ETH",
                Quantity = 2
            }
        };
        var priceSnapshotModelList = new List<PriceSnapshotModel>
        {
            new PriceSnapshotModel
            {
                Name = "BTC",
                Last = 10000
            },
            new PriceSnapshotModel
            {
                Name = "ETH",
                Last = 500
            }
        };
        var exchange = "CoinSpot";

        // Act
        var result = positionModelList.MapToPortfolioModel(priceSnapshotModelList, exchange, now);

        // Assert
        Assert.Equal(exchange, result.Exchange);
        Assert.Equal(11000, result.TotalValue);
        Assert.Equal(2, result.Positions.Count);
        Assert.Equal("BTC", result.Positions[0].Name);
        Assert.Equal(1, result.Positions[0].Quantity);
        Assert.Equal(10000, result.Positions[0].CurrentPrice);
        Assert.Equal(now, result.Positions[0].Timestamp);
        Assert.Equal("ETH", result.Positions[1].Name);
        Assert.Equal(2, result.Positions[1].Quantity);
        Assert.Equal(500, result.Positions[1].CurrentPrice);
        Assert.Equal(now, result.Positions[1].Timestamp);
    }
}