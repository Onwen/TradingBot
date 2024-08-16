namespace TradingBot.Domain.Repository.Order;

public record OrderDto(
    string Id,
    string Exchange,
    string OrderType,
    string Coin,
    string Market,
    decimal Rate,
    decimal Amount,
    bool Cancelled,
    decimal? Total,
    DateTime? SoldDate,
    decimal? AudFeeExGst,
    decimal? AudGst,
    decimal? AudTotal,
    DateTime? Timestamp);
