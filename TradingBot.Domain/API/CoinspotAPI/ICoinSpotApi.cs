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
    Task<PlaceMarketSellOrderResponse> PlaceMarketSellOrder([Body]PlaceMarketSellOrderRequest request);
    [Post("/api/v2/my/buy")]
    Task<PlaceMarketBuyOrderResponse> PlaceMarketBuyOrder([Body]PlaceMarketBuyOrderRequest request);
    [Post("/api/v2/my/orders/market/completed")]
    Task<GetCompletedMarketOrdersResponse> GetCompletedMarketOrders([Body]GetCompletedMarketOrdersRequest request);
    [Post("/api/v2/my/sell/cancel")]
    Task<CancelMarketOrderResponse> CancelMarketSellOrder([Body]CancelMarketOrderRequest request);
    [Post("/api/v2/my/buy/cancel")]
    Task<CancelMarketOrderResponse> CancelMarketBuyOrder([Body]CancelMarketOrderRequest request);
}