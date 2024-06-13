using System.Net;
using Moq;
using Moq.Protected;
using Refit;
using TradingBot.Domain.API.CoinSpotAPI;
using TradingBot.Domain.API.CoinSpotAPI.Request;

namespace TradingBot.Domain.Tests.API;

public class CoinspotAPITests
{
    [Fact]
    public async Task GetBalancesAsync_ShouldParseResponseCorrectly()
    {
        // Arrange
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();

        var jsonResponse = @"
        {
           ""status"":""ok"",
           ""message"":""ok"",
           ""balances"":[
              {
                 ""AUD"":{
                    ""balance"":1000.11,
                    ""audbalance"":1000.11,
                    ""rate"":1
                 }
              },
              {
                 ""BTC"":{
                    ""balance"":1.1111111,
                    ""audbalance"":2222.22,
                    ""rate"":111111.11
                 }
              },
              {
                 ""LTC"":{
                    ""balance"":111.111111,
                    ""audbalance"":22222.22,
                    ""rate"":11.1111
                 }
              }]
        }";

        mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(jsonResponse)
            });

        var httpClient = new HttpClient(mockHttpMessageHandler.Object)
        {
            BaseAddress = new Uri("https://www.coinspot.com.au")
        };

        var apiClient = RestService.For<ICoinSpotApi>(httpClient);

        // Act
        var response = await apiClient.GetMyBalances(new GetMyBalancesRequest());

        // Assert
        Assert.NotNull(response);
        Assert.Equal("ok", response.Status);
        Assert.Equal("ok", response.Message);
        Assert.NotNull(response.Balances);
        Assert.Equal(response.Balances.Count, 3);
        Assert.True(response.Balances[0].ContainsKey("AUD"));
        Assert.True(response.Balances[1].ContainsKey("BTC"));
        Assert.True(response.Balances[2].ContainsKey("LTC"));

        var audBalance = response.Balances[0]["AUD"];
        Assert.Equal(1000.11m, audBalance.Balance);
        Assert.Equal(1000.11m, audBalance.AudBalance);
        Assert.Equal(1m, audBalance.Rate);

        var btcBalance = response.Balances[1]["BTC"];
        Assert.Equal(1.1111111m, btcBalance.Balance);
        Assert.Equal(2222.22m, btcBalance.AudBalance);
        Assert.Equal(111111.11m, btcBalance.Rate);

        var ltcBalance = response.Balances[2]["LTC"];
        Assert.Equal(111.111111m, ltcBalance.Balance);
        Assert.Equal(22222.22m, ltcBalance.AudBalance);
        Assert.Equal(11.1111m, ltcBalance.Rate);
    }
    // test GetLatestPricesAsync
    [Fact]
    public async Task GetLatestPricesAsync_ShouldParseResponseCorrectly()
    {
        // Arrange
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();

        var jsonResponse = @"
        {
           ""status"":""ok"",
           ""message"":""ok"",
            ""Prices"":{
                ""btc"":{
                    ""Bid"":""102000"",
                    ""Ask"":""102374.28895123"",
                    ""Last"":""102200""
                },
		        ""btc_usdt"":{
			        ""bid"":""11111"",
			        ""ask"":""222222"",
			        ""last"":""1111.11""
		        },
		        ""ltc"":{
			        ""Bid"":""1.11111"",
			        ""Ask"":""111"",
			        ""Last"":""111""
		        },
		        ""doge"":{
			        ""bid"":""1.111111"",
			        ""ask"":""1.111111"",
			        ""last"":""1.11111""
		        }
            }
        }";

        mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(jsonResponse)
            });

        var httpClient = new HttpClient(mockHttpMessageHandler.Object)
        {
            BaseAddress = new Uri("https://www.coinspot.com.au")
        };

        var apiClient = RestService.For<ICoinSpotApi>(httpClient);

        // Act
        var response = await apiClient.GetLatestPrices(new GetLatestPricesRequest());

        // Assert
        Assert.NotNull(response);
        Assert.Equal("ok", response.Status);
        Assert.Equal("ok", response.Message);
        Assert.NotNull(response.Prices);
        Assert.Equal(response.Prices.Count, 4);
        
        Assert.True(response.Prices.ContainsKey("btc"));
        var btcPrice = response.Prices["btc"];
        Assert.Equal("102000", btcPrice.Bid);
        Assert.Equal("102374.28895123", btcPrice.Ask);
        Assert.Equal("102200", btcPrice.Last);
        
        Assert.True(response.Prices.ContainsKey("btc_usdt"));
        var btcUsdtPrice = response.Prices["btc_usdt"];
        Assert.Equal("11111", btcUsdtPrice.Bid);
        Assert.Equal("222222", btcUsdtPrice.Ask);
        
        Assert.True(response.Prices.ContainsKey("ltc"));
        var ltcPrice = response.Prices["ltc"];
        Assert.Equal("1.11111", ltcPrice.Bid);
        Assert.Equal("111", ltcPrice.Ask);
        Assert.Equal("111", ltcPrice.Last);
        
        Assert.True(response.Prices.ContainsKey("doge"));
        var dogePrice = response.Prices["doge"];
        Assert.Equal("1.111111", dogePrice.Bid);
        Assert.Equal("1.111111", dogePrice.Ask);
        Assert.Equal("1.11111", dogePrice.Last);
    }
    
    [Fact]
    public async Task PlaceMarketBuyNowOrderAsync_ShouldParseResponseCorrectly()
    {
        // Arrange
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();

        var jsonResponse = @"{
           ""status"":""ok"",
           ""message"":""ok"",
           ""coin"":""BTC"",
           ""market"": ""BTC/AUD"",
           ""amount"":1.234,
           ""rate"":123.344,
           ""id"":""12345678901234567890""
        }";

        mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(jsonResponse)
            });

        var httpClient = new HttpClient(mockHttpMessageHandler.Object)
        {
            BaseAddress = new Uri("https://www.coinspot.com.au")
        };

        var apiClient = RestService.For<ICoinSpotApi>(httpClient);

        // Act
        var response = await apiClient.PlaceMarketBuyNowOrder(new ()
        {
            Amount = 1.234m,
            CoinType = "BTC",
            AmountType = "AUD",
        });

        // Assert
        Assert.NotNull(response);
        Assert.Equal("ok", response.Status);
        Assert.Equal("ok", response.Message);
        Assert.Equal("BTC", response.Coin);
        Assert.Equal("BTC/AUD", response.Market);
        Assert.Equal(1.234m, response.Amount);
        Assert.Equal(123.344m, response.Rate);
        Assert.Equal("12345678901234567890", response.Id);
    }
    
    
    [Fact]
    public async Task PlaceMarketSellNowOrderAsync_ShouldParseResponseCorrectly()
    {
        // Arrange
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();

        var jsonResponse = @"
        {
           ""status"":""ok"",
           ""message"":""ok"",
           ""coin"":""BTC"",
           ""market"": ""BTC/AUD"",
           ""amount"":1.234,
           ""rate"":123.344,
           ""id"":""12345678901234567890""
        }";

        mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(jsonResponse)
            });

        var httpClient = new HttpClient(mockHttpMessageHandler.Object)
        {
            BaseAddress = new Uri("https://www.coinspot.com.au")
        };

        var apiClient = RestService.For<ICoinSpotApi>(httpClient);

        // Act
        var response = await apiClient.PlaceMarketSellNowOrder(new ());

        // Assert
        Assert.NotNull(response);
        Assert.Equal("ok", response.Status);
        Assert.Equal("ok", response.Message);
        Assert.Equal("BTC", response.Coin);
        Assert.Equal("BTC/AUD", response.Market);
        Assert.Equal(1.234m, response.Amount);
        Assert.Equal(123.344m, response.Rate);
        Assert.Equal("12345678901234567890", response.Id);
    }
}