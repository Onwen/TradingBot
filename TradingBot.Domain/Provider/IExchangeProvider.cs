using TradingBot.Domain.Model;

namespace TradingBot.Domain.Provider;

public interface IExchangeProvider
{
    List<TickerData> GetTickers(List<string> tickerNames);
    Portfolio GetPortfolio(string exchange);
}