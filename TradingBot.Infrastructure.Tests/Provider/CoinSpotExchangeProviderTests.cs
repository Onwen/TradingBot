using Microsoft.Extensions.Logging;
using Moq;
using TradingBot.Domain.API.CoinSpotAPI;
using TradingBot.Domain.API.CoinSpotAPI.Request;
using TradingBot.Domain.API.CoinSpotAPI.Response;
using TradingBot.Domain.Model;
using TradingBot.Domain.TimeProvider;
using TradingBot.Infrastructure.Provider;

namespace TradingBot.Infrastructure.Tests.Provider;

public class CoinSpotExchangeProviderTests
{
    private readonly Mock<ICoinSpotApi> _coinSpotApi;
    private readonly StaticTimeProvider _timeProvider;
    private readonly Mock<ILogger<CoinSpotExchangeProvider>> _logger;
    private readonly CoinSpotExchangeProvider _coinSpotExchangeProvider;
    public CoinSpotExchangeProviderTests()
    {
        _coinSpotApi = new Mock<ICoinSpotApi>();
        _timeProvider = new StaticTimeProvider(DateTimeOffset.UtcNow);
        _logger = new Mock<ILogger<CoinSpotExchangeProvider>>();
        _coinSpotExchangeProvider = new CoinSpotExchangeProvider(_coinSpotApi.Object, _timeProvider, _logger.Object);
    }

    #region GetTicker
    [Fact]
    public async void GetTicker_Success()
    {
        // Arrange
        var response = new GetLatestPricesResponse()
        {
            Status = "ok",
            Prices = new Dictionary<string, PriceDetail>()
            {
                {"BTC", new PriceDetail() {Bid = "1", Ask = "2", Last = "3"}},
                {"ETH", new PriceDetail() {Bid = "4", Ask = "5", Last = "6"}}
            }
        };
        _coinSpotApi.Setup(x => x.GetLatestPrices(It.IsAny<GetLatestPricesRequest>())).ReturnsAsync(response);

        // Act
        var result = await _coinSpotExchangeProvider.GetTicker("BTC");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("BTC", result.Name);
        Assert.Equal(1, result.Bid);
        Assert.Equal(2, result.Ask);
        Assert.Equal(3, result.Last);
    }
    
    [Fact]
    public async void GetTicker_Failed()
    {
        // Arrange
        var response = new GetLatestPricesResponse()
        {
            Status = "error",
            Prices = null
        };
        _coinSpotApi.Setup(x => x.GetLatestPrices(It.IsAny<GetLatestPricesRequest>())).ReturnsAsync(response);

        // Act
        await Assert.ThrowsAsync<Exception>(() => _coinSpotExchangeProvider.GetTicker("BTC"));
    }

    #endregion
    #region GetPriceSnapshots
    // Add get tickers success test
    [Fact]
    public async void GetTickers_Success()
    {
        // Arrange
        var response = new GetLatestPricesResponse()
        {
            Status = "ok",
            Prices = new Dictionary<string, PriceDetail>()
            {
                {"BTC", new PriceDetail() {Bid = "1", Ask = "2", Last = "3"}},
                {"ETH", new PriceDetail() {Bid = "4", Ask = "5", Last = "6"}}
            }
        };
        _coinSpotApi.Setup(x => x.GetLatestPrices(It.IsAny<GetLatestPricesRequest>())).ReturnsAsync(response);

        // Act
        var result = await _coinSpotExchangeProvider.GetPriceSnapshots();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Equal("BTC", result[0].Name);
        Assert.Equal(1, result[0].Bid);
        Assert.Equal(2, result[0].Ask);
        Assert.Equal(3, result[0].Last);
        Assert.Equal("ETH", result[1].Name);
        Assert.Equal(4, result[1].Bid);
        Assert.Equal(5, result[1].Ask);
        Assert.Equal(6, result[1].Last);
    }
    
    // Add get tickers failed test
    [Fact]
    public async void GetTickers_Failed()
    {
        // Arrange
        var response = new GetLatestPricesResponse()
        {
            Status = "error",
            Prices = null
        };
        _coinSpotApi.Setup(x => x.GetLatestPrices(It.IsAny<GetLatestPricesRequest>())).ReturnsAsync(response);

        // Act
        await Assert.ThrowsAsync<Exception>(() => _coinSpotExchangeProvider.GetPriceSnapshots());
    }
    
    #endregion
    #region MarketBuy
    [Fact]
    public async void Buy_Success()
    {
        // Arrange
        var getLatestPricesResponse = new GetLatestPricesResponse()
        {
            Status = "ok",
            Prices = new Dictionary<string, PriceDetail>()
            {
                {"BTC", new PriceDetail() {Bid = "1", Ask = "2", Last = "3"}},
                {"ETH", new PriceDetail() {Bid = "4", Ask = "5", Last = "6"}}
            }
        };
        var getMyBalancesResponse = new GetMyBalancesResponse()
        {
            Status = "ok",
            Balances =
            [
                new()
                {
                    { "AUD", new BalanceDetail() { Balance = 2 } }
                }
            ]
        };
        var response = new PlaceMarketBuyOrderResponse()
        {
            Status = "ok",
            Coin = "BTC",
            Amount = 1,
            Market = "BTC/AUD",
            Rate = 1,
            Id = "1",
        };
        _coinSpotApi.Setup(x => x.GetLatestPrices(It.IsAny<GetLatestPricesRequest>())).ReturnsAsync(getLatestPricesResponse);
        _coinSpotApi.Setup(x => x.GetMyBalances(It.IsAny<GetMyBalancesRequest>())).ReturnsAsync(getMyBalancesResponse);
        _coinSpotApi.Setup(x => x.PlaceMarketBuyOrder(It.IsAny<PlaceMarketBuyOrderRequest>())).ReturnsAsync(response);

        // Act
        var result = await _coinSpotExchangeProvider.MarketBuy("BTC", 1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("CoinSpot", result.Exchange);
        Assert.Equal("BUY", result.OrderType);
        Assert.Equal("BTC", result.Coin);
        Assert.Equal(1, result.Amount);
        Assert.Equal("BTC/AUD", result.Market);
        Assert.Equal(1, result.Rate);
        Assert.Equal("1", result.Id);
    }
    
    [Fact]
    public async void Buy_Failed()
    {
        // Arrange
        var getLatestPricesResponse = new GetLatestPricesResponse()
        {
            Status = "ok",
            Prices = new Dictionary<string, PriceDetail>()
            {
                {"BTC", new PriceDetail() {Bid = "1", Ask = "2", Last = "3"}},
                {"ETH", new PriceDetail() {Bid = "4", Ask = "5", Last = "6"}}
            }
        };
        var getMyBalancesResponse = new GetMyBalancesResponse()
        {
            Status = "ok",
            Balances =
            [
                new()
                {
                    { "AUD", new BalanceDetail() { Balance = 1 } }
                }
            ]
        };
        var placeMarketBuyOrderResponse = new PlaceMarketBuyOrderResponse()
        {
            Status = "error"
        };
        _coinSpotApi.Setup(x => x.GetLatestPrices(It.IsAny<GetLatestPricesRequest>())).ReturnsAsync(getLatestPricesResponse);
        _coinSpotApi.Setup(x => x.GetMyBalances(It.IsAny<GetMyBalancesRequest>())).ReturnsAsync(getMyBalancesResponse);
        _coinSpotApi.Setup(x => x.PlaceMarketBuyOrder(It.IsAny<PlaceMarketBuyOrderRequest>())).ReturnsAsync(placeMarketBuyOrderResponse);

        // Act
        await Assert.ThrowsAsync<Exception>(() => _coinSpotExchangeProvider.MarketBuy("BTC", 1));
    }
    // Add buy failed test due to insufficient balance
    [Fact]
    public async void Buy_InsufficientBalance()
    {
        // Arrange
        var getLatestPricesResponse = new GetLatestPricesResponse()
        {
            Status = "ok",
            Prices = new Dictionary<string, PriceDetail>()
            {
                {"BTC", new PriceDetail() {Bid = "1", Ask = "2", Last = "3"}},
                {"ETH", new PriceDetail() {Bid = "4", Ask = "5", Last = "6"}}
            }
        };
        var getMyBalancesResponse = new GetMyBalancesResponse()
        {
            Status = "ok",
            Balances =
            [
                new()
                {
                    { "AUD", new BalanceDetail() { Balance = 1 } }
                }
            ]
        };
        _coinSpotApi.Setup(x => x.GetLatestPrices(It.IsAny<GetLatestPricesRequest>())).ReturnsAsync(getLatestPricesResponse);
        _coinSpotApi.Setup(x => x.GetMyBalances(It.IsAny<GetMyBalancesRequest>())).ReturnsAsync(getMyBalancesResponse);

        // Act
        await Assert.ThrowsAsync<Exception>(() => _coinSpotExchangeProvider.MarketBuy("BTC", 2));
    }
    #endregion
    #region MarketSell
    [Fact]
    public async void Sell_Success()
    {
        // Arrange
        var getLatestPricesResponse = new GetLatestPricesResponse()
        {
            Status = "ok",
            Prices = new Dictionary<string, PriceDetail>()
            {
                {"BTC", new PriceDetail() {Bid = "1", Ask = "2", Last = "3"}},
                {"ETH", new PriceDetail() {Bid = "4", Ask = "5", Last = "6"}}
            }
        };
        var getMyBalancesResponse = new GetMyBalancesResponse()
        {
            Status = "ok",
            Balances =
            [
                new()
                {
                    { "BTC", new BalanceDetail() { Balance = 1 } }
                }
            ]
        };
        var response = new PlaceMarketSellOrderResponse()
        {
            Status = "ok",
            Coin = "BTC",
            Amount = 1,
            Market = "BTC/AUD",
            Rate = 1,
            Id = "1",
        };
        _coinSpotApi.Setup(x => x.GetLatestPrices(It.IsAny<GetLatestPricesRequest>())).ReturnsAsync(getLatestPricesResponse);
        _coinSpotApi.Setup(x => x.GetMyBalances(It.IsAny<GetMyBalancesRequest>())).ReturnsAsync(getMyBalancesResponse);
        _coinSpotApi.Setup(x => x.PlaceMarketSellOrder(It.IsAny<PlaceMarketSellOrderRequest>())).ReturnsAsync(response);

        // Act
        var result = await _coinSpotExchangeProvider.MarketSell("BTC", 1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("CoinSpot", result.Exchange);
        Assert.Equal("SELL", result.OrderType);
        Assert.Equal("BTC", result.Coin);
        Assert.Equal(1, result.Amount);
        Assert.Equal("BTC/AUD", result.Market);
        Assert.Equal(1, result.Rate);
        Assert.Equal("1", result.Id);
    }
    
    [Fact]
    public async void Sell_Failed()
    {
        // Arrange
        var getLatestPricesResponse = new GetLatestPricesResponse()
        {
            Status = "ok",
            Prices = new Dictionary<string, PriceDetail>()
            {
                {"BTC", new PriceDetail() {Bid = "1", Ask = "2", Last = "3"}},
                {"ETH", new PriceDetail() {Bid = "4", Ask = "5", Last = "6"}}
            }
        };
        var getMyBalancesResponse = new GetMyBalancesResponse()
        {
            Status = "ok",
            Balances =
            [
                new()
                {
                    { "BTC", new BalanceDetail() { Balance = 1 } }
                }
            ]
        };
        var response = new PlaceMarketSellOrderResponse()
        {
            Status = "error"
        };
        _coinSpotApi.Setup(x => x.GetLatestPrices(It.IsAny<GetLatestPricesRequest>())).ReturnsAsync(getLatestPricesResponse);
        _coinSpotApi.Setup(x => x.GetMyBalances(It.IsAny<GetMyBalancesRequest>())).ReturnsAsync(getMyBalancesResponse);
        _coinSpotApi.Setup(x => x.PlaceMarketSellOrder(It.IsAny<PlaceMarketSellOrderRequest>())).ReturnsAsync(response);

        // Act
        await Assert.ThrowsAsync<Exception>(() => _coinSpotExchangeProvider.MarketSell("BTC", 1));
    }
    // Add sell failed test due to insufficient balance
    [Fact]
    public async void Sell_InsufficientBalance()
    {
        // Arrange
        var getLatestPricesResponse = new GetLatestPricesResponse()
        {
            Status = "ok",
            Prices = new Dictionary<string, PriceDetail>()
            {
                {"BTC", new PriceDetail() {Bid = "1", Ask = "2", Last = "3"}},
                {"ETH", new PriceDetail() {Bid = "4", Ask = "5", Last = "6"}}
            }
        };
        var getMyBalancesResponse = new GetMyBalancesResponse()
        {
            Status = "ok",
            Balances =
            [
                new()
                {
                    { "BTC", new BalanceDetail() { Balance = 1 } }
                }
            ]
        };
        _coinSpotApi.Setup(x => x.GetLatestPrices(It.IsAny<GetLatestPricesRequest>())).ReturnsAsync(getLatestPricesResponse);
        _coinSpotApi.Setup(x => x.GetMyBalances(It.IsAny<GetMyBalancesRequest>())).ReturnsAsync(getMyBalancesResponse);

        // Act
        await Assert.ThrowsAsync<Exception>(() => _coinSpotExchangeProvider.MarketSell("BTC", 2));
    }
    #endregion
    #region GetCompletedMarketOrders
    // Add get completed market orders success test
    [Fact]
    public async void GetCompletedMarketOrders_Success()
    {
        // Arrange
        var now = DateTimeOffset.UtcNow;
        var response = new GetCompletedMarketOrdersResponse()
        {
            Status = "ok",
            BuyOrders = new List<Order>()
            {
                new() { Coin = "BTC", Amount = 1, Rate = 1, Market = "BTC/AUD", Id = "1", SoldDate = now, AudGst = 1, AudFeeExGst = 1, AudTotal = 1 },
                new() { Coin = "ETH", Amount = 2, Rate = 2, Market = "ETH/AUD", Id = "2", SoldDate = now, AudGst = 2, AudFeeExGst = 2, AudTotal = 2 }
            },
            SellOrders = new List<Order>()
            {
                new() { Coin = "BTC", Amount = 1, Rate = 1, Market = "BTC/AUD", Id = "1", SoldDate = now, AudGst = 1, AudFeeExGst = 1, AudTotal = 1 },
                new() { Coin = "ETH", Amount = 2, Rate = 2, Market = "ETH/AUD", Id = "2", SoldDate = now, AudGst = 2, AudFeeExGst = 2, AudTotal = 2 }
            }
        };
        _coinSpotApi.Setup(x => x.GetCompletedMarketOrders(It.IsAny<GetCompletedMarketOrdersRequest>())).ReturnsAsync(response);

        // Act
        var result = await _coinSpotExchangeProvider.GetCompletedMarketOrders();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(4, result.Count);
        Assert.Equal("BTC", result[0].Coin);
        Assert.Equal(1, result[0].Amount);
        Assert.Equal(1, result[0].Rate);
        Assert.Equal("BTC/AUD", result[0].Market);
        Assert.Equal("1", result[0].Id);
        Assert.Equal(now, result[0].SoldDate);
        Assert.Equal(1, result[0].AudGst);
        Assert.Equal(1, result[0].AudFeeExGst);
        Assert.Equal(1, result[0].AudTotal);
        Assert.Equal("BUY", result[0].OrderType);
        Assert.Equal("ETH", result[3].Coin);
        Assert.Equal(2, result[3].Amount);
        Assert.Equal(2, result[3].Rate);
        Assert.Equal("ETH/AUD", result[3].Market);
        Assert.Equal("2", result[3].Id);
        Assert.Equal(now, result[3].SoldDate);
        Assert.Equal(2, result[3].AudGst);
        Assert.Equal(2, result[3].AudFeeExGst);
        Assert.Equal(2, result[3].AudTotal);
        Assert.Equal("SELL", result[3].OrderType);
    }
    // Add get completed market orders failed test
    [Fact]
    public async void GetCompletedMarketOrders_Failed()
    {
        // Arrange
        var response = new GetCompletedMarketOrdersResponse()
        {
            Status = "error",
            BuyOrders = [],
            SellOrders = []
        };
        _coinSpotApi.Setup(x => x.GetCompletedMarketOrders(It.IsAny<GetCompletedMarketOrdersRequest>())).ReturnsAsync(response);

        // Act
        await Assert.ThrowsAsync<Exception>(() => _coinSpotExchangeProvider.GetCompletedMarketOrders());
    }
    #endregion
    #region CancelMarketOrder
    // Add cancel market order success test handling both buy and sell orders
    [Theory]
    [InlineData("BUY")]
    [InlineData("SELL")]
    public async void CancelMarketOrder_Success(string orderType)
    {
        // Arrange
        var response = new CancelMarketOrderResponse()
        {
            Status = "ok"
        };
        _coinSpotApi.Setup(x => x.CancelMarketBuyOrder(It.IsAny<CancelMarketOrderRequest>())).ReturnsAsync(response);
        _coinSpotApi.Setup(x => x.CancelMarketSellOrder(It.IsAny<CancelMarketOrderRequest>())).ReturnsAsync(response);

        // Act
        var result = await _coinSpotExchangeProvider.CancelMarketOrder("1", orderType);

        // Assert
        Assert.True(result);
    }
    // Add cancel market order failed test handling both buy and sell orders
    [Theory]
    [InlineData("BUY")]
    [InlineData("SELL")]
    public async void CancelMarketOrder_Failed(string orderType)
    {
        // Arrange
        var response = new CancelMarketOrderResponse()
        {
            Status = "error"
        };
        _coinSpotApi.Setup(x => x.CancelMarketBuyOrder(It.IsAny<CancelMarketOrderRequest>())).ReturnsAsync(response);
        _coinSpotApi.Setup(x => x.CancelMarketSellOrder(It.IsAny<CancelMarketOrderRequest>())).ReturnsAsync(response);

        // Act
        await Assert.ThrowsAsync<Exception>(() => _coinSpotExchangeProvider.CancelMarketOrder("1", orderType));
    }
    #endregion
    #region GetPortfolio
    [Fact]
    public async void GetPortfolio_Success()
    {
        // Arrange
        var response = new GetMyBalancesResponse()
        {
            Status = "ok",
            Balances =
            [
                new()
                {
                    { "BTC", new BalanceDetail() { Balance = 1 } }
                },
                new()
                {
                    { "ETH", new BalanceDetail() { Balance = 2 } }
                }
            ]
        };
        _coinSpotApi.Setup(x => x.GetMyBalances(It.IsAny<GetMyBalancesRequest>())).ReturnsAsync(response);

        // Act
        var result = await _coinSpotExchangeProvider.GetPortfolio();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Equal("BTC", result[0].Name);
        Assert.Equal(1, result[0].Quantity);
        Assert.Equal("ETH", result[1].Name);
        Assert.Equal(2, result[1].Quantity);
    }
    
    // write a test where ICoinSpotAPI.GetMyBalances returns a response with null balances and status "ok"
    [Fact]
    public async void GetPortfolio_NoBalances_Success()
    {
        // Arrange
        var response = new GetMyBalancesResponse()
        {
            Status = "ok",
            Balances = null
        };
        _coinSpotApi.Setup(x => x.GetMyBalances(It.IsAny<GetMyBalancesRequest>())).ReturnsAsync(response);

        // Act
        var result = await _coinSpotExchangeProvider.GetPortfolio();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }
    
    // write a test where ICoinSpotAPI.GetMyBalances returns a response with status "error"
    [Fact]
    public async void GetPortfolio_Failed()
    {
        // Arrange
        var response = new GetMyBalancesResponse()
        {
            Status = "error",
            Balances = null
        };
        _coinSpotApi.Setup(x => x.GetMyBalances(It.IsAny<GetMyBalancesRequest>())).ReturnsAsync(response);

        // Act
        await Assert.ThrowsAsync<Exception>(() => _coinSpotExchangeProvider.GetPortfolio());
    }
    #endregion
}