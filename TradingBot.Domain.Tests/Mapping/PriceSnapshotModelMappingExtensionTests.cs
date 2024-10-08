using TradingBot.Domain.Enum;
using TradingBot.Domain.Mapping;
using TradingBot.Domain.Model;

namespace TradingBot.Domain.Tests.Mapping;

public class PriceSnapshotModelMappingExtensionTests
{
    // Test the mapping of the PriceSnapshotModel to the PriceSnapshotDto
    [Fact]
    public void MapToPriceSnapshotDto_Success()
    {
        // Arrange
        var now = DateTimeOffset.UtcNow;
        var tickerModel = new PriceSnapshotModel
        {
            Name = "BTC",
            Ask = 1,
            Bid = 2,
            Last = 3
        };

        // Act
        var result = tickerModel.MapToPriceSnapshotDto(now);

        // Assert
        Assert.Equal("BTC", result.Name);
        Assert.Equal(Currency.AUD, result.Currency);
        Assert.Equal(1, result.Ask);
        Assert.Equal(2, result.Bid);
        Assert.Equal(3, result.Last);
        Assert.Equal(now, result.Timestamp);
    }
    // Test the mapping of the PriceSnapshotModel to the PriceSnapshotDto with null PriceSnapshotModel
    [Fact]
    public void MapToPriceSnapshotDto_NullTickerModel()
    {
        // Arrange
        var now = DateTimeOffset.UtcNow;
        PriceSnapshotModel priceSnapshotModel = null;

        // Act
        var result = priceSnapshotModel.MapToPriceSnapshotDto(now);

        // Assert
        Assert.Null(result);
    }
    // Test the mapping of List<PriceSnapshotModel> to List<PriceSnapshotDto>
    [Fact]
    public void MapToPriceSnapshotDto_List_Success()
    {
        // Arrange
        var now = DateTimeOffset.UtcNow;
        var tickerModelList = new List<PriceSnapshotModel>
        {
            new PriceSnapshotModel
            {
                Name = "BTC",
                Ask = 1,
                Bid = 2,
                Last = 3
            },
            new PriceSnapshotModel
            {
                Name = "ETH",
                Ask = 4,
                Bid = 5,
                Last = 6
            }
        };

        // Act
        var result = tickerModelList.MapToPriceSnapshotDto(now);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal("BTC", result[0].Name);
        Assert.Equal(Currency.AUD, result[0].Currency);
        Assert.Equal(1, result[0].Ask);
        Assert.Equal(2, result[0].Bid);
        Assert.Equal(3, result[0].Last);
        Assert.Equal(now, result[0].Timestamp);
        Assert.Equal("ETH", result[1].Name);
        Assert.Equal(Currency.AUD, result[1].Currency);
        Assert.Equal(4, result[1].Ask);
        Assert.Equal(5, result[1].Bid);
        Assert.Equal(6, result[1].Last);
        Assert.Equal(now, result[0].Timestamp);
    }
    // Test the mapping of List<PriceSnapshotModel> to List<PriceSnapshotDto> with null PriceSnapshotModel
    [Fact]
    public void MapToPriceSnapshotDto_List_NullTickerModel()
    {
        // Arrange
        var now = DateTimeOffset.UtcNow;
        List<PriceSnapshotModel> tickerModelList = null;

        // Act
        var result = tickerModelList.MapToPriceSnapshotDto(now);

        // Assert
        Assert.Empty(result);
    }
    // Test the mapping of List<PriceSnapshotModel> to List<PriceSnapshotDto> with empty PriceSnapshotModel
    [Fact]
    public void MapToPriceSnapshotDto_List_EmptyTickerModel()
    {
        // Arrange
        var now = DateTimeOffset.UtcNow;
        var tickerModelList = new List<PriceSnapshotModel>();

        // Act
        var result = tickerModelList.MapToPriceSnapshotDto(now);

        // Assert
        Assert.Empty(result);
    }
}