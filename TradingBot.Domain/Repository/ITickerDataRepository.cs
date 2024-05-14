using TradingBot.Domain.Model;

namespace TradingBot.Domain;

public interface ITickerDataRepository
{
    TickerData GetTickers(List<string> name);
}