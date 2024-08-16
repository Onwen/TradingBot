namespace TradingBot.Domain.Repository.PriceHistory;

public record PriceHistoryDto(string Name, string Symbol, DateTime TimeOpen, decimal Open, DateTime TimeHigh, decimal High, DateTime TimeLow, decimal Low, DateTime TimeClose, decimal Close, decimal Volume, decimal MarketCap);