using Refit;
using TradingBot.Domain.API.CoinSpotAPI.Request;
using TradingBot.Domain.API.CoinSpotAPI.Response;

namespace TradingBot.Domain.API.CoinSpotAPI;

public interface ICoinSpotApi
{
    [Get("/pubapi/v2/latest")]
    Task<GetLatestPricesResponse> GetLatestPrices([Body]GetLatestPricesRequest request);
    [Post("/api/v2/ro/my/balances")]
    Task<GetMyBalancesResponse> GetMyBalances([Body]GetMyBalancesRequest request);
    [Post("/api/v2/my/sell")]
    Task<PlaceMarketSellNowOrderResponse> PlaceMarketSellNowOrder([Body]PlaceMarketSellNowOrderRequest request);
    [Post("/api/v2/my/buy")]
    Task<PlaceMarketBuyNowOrderResponse> PlaceMarketBuyNowOrder([Body]PlaceMarketBuyNowOrderRequest request);
}