namespace TradingBot.Domain.TimeProvider;

public sealed class StaticTimeProvider(DateTimeOffset utcNow) : System.TimeProvider
{
    public override DateTimeOffset GetUtcNow()
    {
        return utcNow;
    }
}