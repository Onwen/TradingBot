using TradingBot.Domain.Mapping;
using TradingBot.Domain.Model;

namespace TradingBot.Domain.Tests.Mapping;

public class MarketOrderModelMappingExtensionTests
{
    // test mapping MarketOrderModel to OrderDto successfully
    [Fact]
    public void MapToOrderDto_Success()
    {
        // Arrange
        var now = DateTimeOffset.UtcNow;
        var model = new MarketOrderModel
        {
            Id = "id",
            Exchange = "exchange",
            OrderType = "orderType",
            Coin = "coin",
            Market = "market",
            Rate = 1,
            Amount = 1,
            Total = 1,
            SoldDate = now,
            AudFeeExGst = 1,
            AudGst = 1,
            AudTotal = 1,
            Timestamp = now
        };

        // Act
        var result = model.ToOrderDto();

        // Assert
        Assert.Equal(model.Id, result.Id);
        Assert.Equal(model.Exchange, result.Exchange);
        Assert.Equal(model.OrderType, result.OrderType);
        Assert.Equal(model.Coin, result.Coin);
        Assert.Equal(model.Market, result.Market);
        Assert.Equal(model.Rate, result.Rate);
        Assert.Equal(model.Amount, result.Amount);
        Assert.Equal(model.Total, result.Total);
        Assert.Equal(model.SoldDate, result.SoldDate);
        Assert.Equal(model.AudFeeExGst, result.AudFeeExGst);
        Assert.Equal(model.AudGst, result.AudGst);
        Assert.Equal(model.AudTotal, result.AudTotal);
        Assert.Equal(model.Timestamp, result.Timestamp);
    }
}