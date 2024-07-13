namespace TradingBot.Domain.Repository.Return;

public record ReturnDto(string Name, string Exchange, string ReturnType, DateTimeOffset Timestamp, decimal Value)
{
    public int Id { get; init; }
}

public static class ReturnType
{
    public static readonly string Daily = "Daily";
    public static readonly string Monthly = "Monthly";
    public static readonly string Yearly = "Yearly";
}