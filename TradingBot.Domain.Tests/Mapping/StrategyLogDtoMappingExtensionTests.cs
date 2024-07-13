using TradingBot.Domain.Mapping;
using TradingBot.Domain.Repository.StrategyLog;

namespace TradingBot.Domain.Tests.Mapping;

public class StrategyLogDtoMappingExtensionTests
{
    // Test the MapToStrategyLogModel method
    [Fact]
    public void MapToStrategyLogModel_WithValidDto_ReturnsModel()
    {
        // Arrange
        var dto = new StrategyLogDto("TestStrategy", "TestMessage", DateTimeOffset.UtcNow);
        
        // Act
        var model = dto.MapToStrategyLogModel();
        
        // Assert
        Assert.NotNull(model);
        Assert.Equal(dto.StrategyName, model.StrategyName);
        Assert.Equal(dto.Message, model.Message);
        Assert.Equal(dto.Timestamp, model.Timestamp);
    }
    // Test the MapToStrategyLogModel method with a null dto
    [Fact]
    public void MapToStrategyLogModel_WithNullDto_ReturnsNull()
    {
        // Arrange
        StrategyLogDto dto = null;
        
        // Act
        var model = dto.MapToStrategyLogModel();
        
        // Assert
        Assert.Null(model);
    }
    // Test the MapToStrategyLogModel method with an empty list of dtos
    [Fact]
    public void MapToStrategyLogModel_WithEmptyList_ReturnsEmptyList()
    {
        // Arrange
        List<StrategyLogDto> dtos = new();
        
        // Act
        var models = dtos.MapToStrategyLogModel();
        
        // Assert
        Assert.NotNull(models);
        Assert.Empty(models);
    }
    // Test the MapToStrategyLogModel method with a list of dtos
    [Fact]
    public void MapToStrategyLogModel_WithList_ReturnsList()
    {
        // Arrange
        var dtos = new List<StrategyLogDto>
        {
            new StrategyLogDto("TestStrategy1", "TestMessage1", DateTimeOffset.UtcNow),
            new StrategyLogDto("TestStrategy2", "TestMessage2", DateTimeOffset.UtcNow),
            new StrategyLogDto("TestStrategy3", "TestMessage3", DateTimeOffset.UtcNow)
        };
        
        // Act
        var models = dtos.MapToStrategyLogModel();
        
        // Assert
        Assert.NotNull(models);
        Assert.Equal(dtos.Count, models.Count);
        for (var i = 0; i < dtos.Count; i++)
        {
            Assert.Equal(dtos[i].StrategyName, models[i].StrategyName);
            Assert.Equal(dtos[i].Message, models[i].Message);
            Assert.Equal(dtos[i].Timestamp, models[i].Timestamp);
        }
    }
}