using TradingBot.Domain.Mapping;
using TradingBot.Domain.Repository.PriceHistory;

namespace TradingBot.Domain.Tests.Mapping;

public class PriceHistoryDtoMappingExtensionTests
{
    // Test the ToModel method
    [Fact]
    public void ToModelTest()
    {
        // Arrange
        var dto = new PriceHistoryDto(
        "Bitcoin",
            "BTC",
            DateTimeOffset.Now.DateTime,
            10000,
            DateTimeOffset.Now.DateTime,
            11000,
            DateTimeOffset.Now.DateTime,
            9000,
            DateTimeOffset.Now.DateTime,
            10000,
            1000,
            100000);

        // Act
        var model = dto.ToModel();

        // Assert
        Assert.Equal(dto.Name, model.Name);
        Assert.Equal(dto.Symbol, model.Symbol);
        Assert.Equal(dto.TimeOpen, model.TimeOpen);
        Assert.Equal(dto.Open, model.Open);
        Assert.Equal(dto.TimeHigh, model.TimeHigh);
        Assert.Equal(dto.High, model.High);
        Assert.Equal(dto.TimeLow, model.TimeLow);
        Assert.Equal(dto.Low, model.Low);
        Assert.Equal(dto.TimeClose, model.TimeClose);
        Assert.Equal(dto.Close, model.Close);
        Assert.Equal(dto.Volume, model.Volume);
        Assert.Equal(dto.MarketCap, model.MarketCap);
    }
    // test the List<PriceHistoryDto> ToModel method
    [Fact]
    public void ToModelListTest()
    {
        // Arrange
        var dtos = new List<PriceHistoryDto>
        {
            new PriceHistoryDto(
                "Bitcoin",
                "BTC",
                DateTimeOffset.Now.DateTime,
                10000,
                DateTimeOffset.Now.DateTime,
                11000,
                DateTimeOffset.Now.DateTime,
                9000,
                DateTimeOffset.Now.DateTime,
                10000,
                1000,
                100000),
            new PriceHistoryDto(
                "Ethereum",
                "ETH",
                DateTimeOffset.Now.DateTime,
                500,
                DateTimeOffset.Now.DateTime,
                600,
                DateTimeOffset.Now.DateTime,
                400,
                DateTimeOffset.Now.DateTime,
                500,
                500,
                50000)
        };

        // Act
        var models = dtos.ToModel();

        // Assert
        Assert.Equal(dtos.Count, models.Count);
        for (var i = 0; i < dtos.Count; i++)
        {
            Assert.Equal(dtos[i].Name, models[i].Name);
            Assert.Equal(dtos[i].Symbol, models[i].Symbol);
            Assert.Equal(dtos[i].TimeOpen, models[i].TimeOpen);
            Assert.Equal(dtos[i].Open, models[i].Open);
            Assert.Equal(dtos[i].TimeHigh, models[i].TimeHigh);
            Assert.Equal(dtos[i].High, models[i].High);
            Assert.Equal(dtos[i].TimeLow, models[i].TimeLow);
            Assert.Equal(dtos[i].Low, models[i].Low);
            Assert.Equal(dtos[i].TimeClose, models[i].TimeClose);
            Assert.Equal(dtos[i].Close, models[i].Close);
            Assert.Equal(dtos[i].Volume, models[i].Volume);
            Assert.Equal(dtos[i].MarketCap, models[i].MarketCap);
        }
    }
}