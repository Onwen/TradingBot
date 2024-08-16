namespace TradingBot.Domain.Repository.PriceHistory;

public interface IPriceHistoryRepository
{
    // Get the latest price history for a given symbol
    Task<PriceHistoryDto?> GetLatestPriceHistory(string symbol);
    // Get the price history for a given symbol
    Task<IEnumerable<PriceHistoryDto>> GetPriceHistory(string symbol);
    // Get the price history for a given symbol and time range
    Task<IEnumerable<PriceHistoryDto>> GetPriceHistory(string symbol, DateTime start, DateTime end);
    // Get the price history for all symbols in a given time range
    Task<IEnumerable<PriceHistoryDto>> GetPriceHistory(DateTime start, DateTime end);
    // save the price history for a given symbol
    Task SavePriceHistory(PriceHistoryDto priceHistory);
    // save the price history for a collection of symbols
    Task SavePriceHistory(IEnumerable<PriceHistoryDto> priceHistory);
}