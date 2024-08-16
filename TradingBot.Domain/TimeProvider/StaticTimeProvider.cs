namespace TradingBot.Domain.TimeProvider;

public sealed class StaticTimeProvider : System.TimeProvider
{
    private DateTimeOffset _currentTime;

    public StaticTimeProvider(DateTimeOffset utcNow)
    {
        _currentTime = utcNow;
    }
    public override DateTimeOffset GetUtcNow()
    {
        return _currentTime;
    }

    public void AdvanceTime(TimeSpan timeSpan)
    {
        _currentTime = _currentTime.Add(timeSpan);
    }
}