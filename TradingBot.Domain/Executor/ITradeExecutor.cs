using TradingBot.Domain.Model;
using TradingBot.Domain.Service;

namespace TradingBot.Domain.Executor;

public interface ITradeExecutor
{
    Task<List<MarketOrderModel>> ExecuteTrades(IPortfolioService portfolioService, IPricingService pricingService, IExchangeService exchangeService, List<TradeSignalModel> tradeSignals);
}