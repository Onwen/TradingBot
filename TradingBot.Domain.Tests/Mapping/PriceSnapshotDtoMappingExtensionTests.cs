using TradingBot.Domain.Enum;
using TradingBot.Domain.Mapping;
using TradingBot.Domain.Repository.Ticker;

namespace TradingBot.Domain.Tests.Mapping;

public class PriceSnapshotDtoMappingExtensionTests
{
    private const string Exchange = "exchange";
    // Test map to price snapshot model
    [Fact]
    public void MapToPriceSnapshotModel_WithValidDto_ReturnsModel()
    {
        // Arrange
        var dto = new PriceSnapshotDto(Exchange, "BTC", Currency.AUD, 10000, 10001, 10000.5m, DateTimeOffset.UtcNow);
        
        // Act
        var model = dto.MapToPriceSnapshotModel();
        
        // Assert
        Assert.NotNull(model);
        Assert.Equal(Exchange, model.Exchange);
        Assert.Equal("BTC", model.Name);
        Assert.Equal(Currency.AUD, model.Currency);
        Assert.Equal(10000, model.Bid);
        Assert.Equal(10001, model.Ask);
        Assert.Equal(10000.5m, model.Last);
        Assert.Equal(dto.Timestamp, model.Timestamp);
    }
    // Test map to price snapshot model with null dto
    [Fact]
    public void MapToPriceSnapshotModel_WithNullDto_ReturnsNull()
    {
        // Arrange
        PriceSnapshotDto dto = null;
        
        // Act
        var model = dto.MapToPriceSnapshotModel();
        
        // Assert
        Assert.Null(model);
    }
    // Test map to price snapshot model list
    [Fact]
    public void MapToPriceSnapshotModel_WithValidDtos_ReturnsModels()
    {
        // Arrange
        var dtos = new List<PriceSnapshotDto>
        {
            new PriceSnapshotDto(Exchange,"BTC", Currency.AUD, 10000, 10001, 10000.5m, DateTimeOffset.UtcNow),
            new PriceSnapshotDto(Exchange,"ETH", Currency.AUD, 500, 501, 500.5m, DateTimeOffset.UtcNow)
        };
        
        // Act
        var models = dtos.MapToPriceSnapshotModel();
        
        // Assert
        Assert.NotNull(models);
        Assert.Equal(2, models.Count);
        Assert.Equal(Exchange, models[0].Exchange);
        Assert.Equal("BTC", models[0].Name);
        Assert.Equal(Currency.AUD, models[0].Currency);
        Assert.Equal(10000, models[0].Bid);
        Assert.Equal(10001, models[0].Ask);
        Assert.Equal(10000.5m, models[0].Last);
        Assert.Equal(dtos[0].Timestamp, models[0].Timestamp);
        Assert.Equal(Exchange, models[1].Exchange);
        Assert.Equal("ETH", models[1].Name);
        Assert.Equal(Currency.AUD, models[1].Currency);
        Assert.Equal(500, models[1].Bid);
        Assert.Equal(501, models[1].Ask);
        Assert.Equal(500.5m, models[1].Last);
        Assert.Equal(dtos[1].Timestamp, models[1].Timestamp);
    }
    // Test map to price snapshot model list with null dtos
    [Fact]
    public void MapToPriceSnapshotModel_WithNullDtos_ReturnsEmptyList()
    {
        // Arrange
        List<PriceSnapshotDto> dtos = null;
        
        // Act
        var models = dtos.MapToPriceSnapshotModel();
        
        // Assert
        Assert.NotNull(models);
        Assert.Empty(models);
    }
    // Test map to price snapshot model list with empty dtos
    [Fact]
    public void MapToPriceSnapshotModel_WithEmptyDtos_ReturnsEmptyList()
    {
        // Arrange
        var dtos = new List<PriceSnapshotDto>();
        
        // Act
        var models = dtos.MapToPriceSnapshotModel();
        
        // Assert
        Assert.NotNull(models);
        Assert.Empty(models);
    }
    
}