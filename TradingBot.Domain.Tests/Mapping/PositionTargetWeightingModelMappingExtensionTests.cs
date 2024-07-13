using TradingBot.Domain.Mapping;
using TradingBot.Domain.Model;

namespace TradingBot.Domain.Tests.Mapping;

public class PositionTargetWeightingModelMappingExtensionTests
{
    // Test the mapping of position target weighting model to position target weighting dto
    [Fact]
    public void MapToPositionTargetWeightingDto_Success()
    {
        // Arrange
        var now = DateTimeOffset.UtcNow;
        var positionTargetWeightingModel = new PositionTargetWeightingModel(now)
        {
            Name = "BTC",
            TargetWeighting = 0.5m
        };

        // Act
        var result = positionTargetWeightingModel.MapToPositionTargetWeightingDto();

        // Assert
        Assert.Equal("BTC", result.Name);
        Assert.Equal(0.5m, result.TargetWeighting);
        Assert.Equal(now, result.Timestamp);
    }
    // Test the mapping of position target weighting model to position target weighting dto with null position target weighting model
    [Fact]
    public void MapToPositionTargetWeightingDto_NullPositionTargetWeightingModel()
    {
        // Arrange
        PositionTargetWeightingModel positionTargetWeightingModel = null;

        // Act
        var result = positionTargetWeightingModel.MapToPositionTargetWeightingDto();

        // Assert
        Assert.Null(result);
    }
    // Test the mapping of List<PositionTargetWeightingModel> to List<PositionTargetWeightingDto>
    [Fact]
    public void MapToPositionTargetWeightingDto_List_Success()
    {
        // Arrange
        var now = DateTimeOffset.UtcNow;
        var positionTargetWeightingModelList = new List<PositionTargetWeightingModel>
        {
            new PositionTargetWeightingModel(now)
            {
                Name = "BTC",
                TargetWeighting = 0.5m
            },
            new PositionTargetWeightingModel(now)
            {
                Name = "ETH",
                TargetWeighting = 0.3m
            }
        };

        // Act
        var result = positionTargetWeightingModelList.MapToPositionTargetWeightingDtos();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Equal("BTC", result[0].Name);
        Assert.Equal(0.5m, result[0].TargetWeighting);
        Assert.Equal(now, result[0].Timestamp);
        Assert.Equal("ETH", result[1].Name);
        Assert.Equal(0.3m, result[1].TargetWeighting);
        Assert.Equal(now, result[1].Timestamp);
    }
    // Test the mapping of List<PositionTargetWeightingModel> to List<PositionTargetWeightingDto with null position target weighting model list
    [Fact]
    public void MapToPositionTargetWeightingDto_List_NullPositionTargetWeightingModelList()
    {
        // Arrange
        List<PositionTargetWeightingModel> positionTargetWeightingModelList = null;

        // Act
        var result = positionTargetWeightingModelList.MapToPositionTargetWeightingDtos();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }
    // Test the mapping of List<PositionTargetWeightingModel> to List<PositionTargetWeightingDto with empty position target weighting model list
    [Fact]
    public void MapToPositionTargetWeightingDto_List_EmptyPositionTargetWeightingModelList()
    {
        // Arrange
        var positionTargetWeightingModelList = new List<PositionTargetWeightingModel>();

        // Act
        var result = positionTargetWeightingModelList.MapToPositionTargetWeightingDtos();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }
}