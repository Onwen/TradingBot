using TradingBot.Domain.Mapping;
using TradingBot.Domain.Model;

namespace TradingBot.Domain.Tests.Mapping;

public class StrategyLogModelMappingExtensionTests
{
    // Test the MapToStrategyLogDto method
    [Fact]
    public void MapToStrategyLogDto_WithValidModel_ReturnsDto()
    {
        // Arrange
        var model = new StrategyLogModel(){StrategyName = "TestStrategy", Message = "TestMessage", Timestamp = DateTimeOffset.UtcNow};
        
        // Act
        var dto = model.MapToStrategyLogDto();
        
        // Assert
        Assert.NotNull(dto);
        Assert.Equal(model.StrategyName, dto.StrategyName);
        Assert.Equal(model.Message, dto.Message);
        Assert.Equal(model.Timestamp, dto.Timestamp);
    }
    // Test the MapToStrategyLogDto method with a null model
    [Fact]
    public void MapToStrategyLogDto_WithNullModel_ReturnsNull()
    {
        // Arrange
        StrategyLogModel model = null;
        
        // Act
        var dto = model.MapToStrategyLogDto();
        
        // Assert
        Assert.Null(dto);
    }
    // Test the MapToStrategyLogDto method with an empty list of models
    [Fact]
    public void MapToStrategyLogDto_WithEmptyList_ReturnsEmptyList()
    {
        // Arrange
        List<StrategyLogModel> models = new();
        
        // Act
        var dtos = models.MapToStrategyLogDto();
        
        // Assert
        Assert.NotNull(dtos);
        Assert.Empty(dtos);
    }
    // Test the MapToStrategyLogDto method with a list of models
    [Fact]
    public void MapToStrategyLogDto_WithList_ReturnsList()
    {
        // Arrange
        var models = new List<StrategyLogModel>
        {
            new StrategyLogModel(){StrategyName = "TestStrategy1", Message = "TestMessage1", Timestamp = DateTimeOffset.UtcNow},
            new StrategyLogModel(){StrategyName = "TestStrategy2", Message = "TestMessage2", Timestamp = DateTimeOffset.UtcNow},
            new StrategyLogModel(){StrategyName = "TestStrategy3", Message = "TestMessage3", Timestamp = DateTimeOffset.UtcNow}
        };
        
        // Act
        var dtos = models.MapToStrategyLogDto();
        
        // Assert
        Assert.NotNull(dtos);
        Assert.NotEmpty(dtos);
        Assert.Equal(models.Count, dtos.Count);
        for (int i = 0; i < models.Count; i++)
        {
            Assert.Equal(models[i].StrategyName, dtos[i].StrategyName);
            Assert.Equal(models[i].Message, dtos[i].Message);
            Assert.Equal(models[i].Timestamp, dtos[i].Timestamp);
        }
    }
}