namespace TradingBot.Domain.Executor;

public class BacktestStrategyExecutorConfig : StrategyExecutorConfig
{
    public DateTimeOffset From { get; set; }
    public DateTimeOffset To { get; set; }
    public TimeSpan Interval { get; set; }
}
public class StrategyExecutorConfig
{
    public List<StrategyConfig> StrategyConfigs { get; set; }
}

public class StrategyConfig
{
    public string StrategyName { get; set; }
    public decimal Weight { get; set; }
    public Dictionary<string, string> Config { get; set; }
}
