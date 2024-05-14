using TradingBot.Domain.Enum;

namespace TradingBot.Domain.Model;

public record TickerData(string Name, Currency Currency, decimal Value);