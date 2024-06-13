using Microsoft.Extensions.Logging;
using Moq;
using TradingBot.Domain.API.CoinSpotAPI;
using TradingBot.Domain.API.CoinSpotAPI.Request;
using TradingBot.Domain.API.CoinSpotAPI.Response;
using TradingBot.Infrastructure.Provider;

namespace TradingBot.Infrastructure.Tests.Provider;

public class CoinSpotExchangeProviderTests
{
    private readonly Mock<ICoinSpotApi> _coinSpotApi;
    private readonly Mock<ILogger<CoinSpotExchangeProvider>> _logger;
    private readonly CoinSpotExchangeProvider _coinSpotExchangeProvider;
    public CoinSpotExchangeProviderTests()
    {
        _coinSpotApi = new Mock<ICoinSpotApi>();
        _logger = new Mock<ILogger<CoinSpotExchangeProvider>>();
        _coinSpotExchangeProvider = new CoinSpotExchangeProvider(_coinSpotApi.Object, _logger.Object);
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
    #region GetTickers
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
        var result = await _coinSpotExchangeProvider.GetTickers();

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
        await Assert.ThrowsAsync<Exception>(() => _coinSpotExchangeProvider.GetTickers());
    }
    
    #endregion
    #region Buy
    [Fact]
    public async void Buy_Success()
    {
        // Arrange
        var response = new PlaceMarketBuyNowOrderResponse()
        {
            Status = "ok",
            Coin = "BTC",
            Amount = 1,
            Rate = 1,
            Id = "1",
        };
        _coinSpotApi.Setup(x => x.PlaceMarketBuyNowOrder(It.IsAny<PlaceMarketBuyNowOrderRequest>())).ReturnsAsync(response);

        // Act
        var result = await _coinSpotExchangeProvider.Buy("BTC", 1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("BTC", result.Name);
        Assert.Equal(1, result.Quantity);
    }
    
    [Fact]
    public async void Buy_Failed()
    {
        // Arrange
        var response = new PlaceMarketBuyNowOrderResponse()
        {
            Status = "error"
        };
        _coinSpotApi.Setup(x => x.PlaceMarketBuyNowOrder(It.IsAny<PlaceMarketBuyNowOrderRequest>())).ReturnsAsync(response);

        // Act
        await Assert.ThrowsAsync<Exception>(() => _coinSpotExchangeProvider.Buy("BTC", 1));
    }
    #endregion
    #region Sell
    [Fact]
    public async void Sell_Success()
    {
        // Arrange
        var response = new PlaceMarketSellNowOrderResponse()
        {
            Status = "ok",
            Coin = "BTC",
            Amount = 1,
            Rate = 1,
            Id = "1",
        };
        _coinSpotApi.Setup(x => x.PlaceMarketSellNowOrder(It.IsAny<PlaceMarketSellNowOrderRequest>())).ReturnsAsync(response);

        // Act
        var result = await _coinSpotExchangeProvider.Sell("BTC", 1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("BTC", result.Name);
        Assert.Equal(1, result.Quantity);
    }
    
    [Fact]
    public async void Sell_Failed()
    {
        // Arrange
        var response = new PlaceMarketSellNowOrderResponse()
        {
            Status = "error"
        };
        _coinSpotApi.Setup(x => x.PlaceMarketSellNowOrder(It.IsAny<PlaceMarketSellNowOrderRequest>())).ReturnsAsync(response);

        // Act
        await Assert.ThrowsAsync<Exception>(() => _coinSpotExchangeProvider.Sell("BTC", 1));
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