namespace TradingBot.Domain.Model;

// create a model for the position target weighting
public class PositionTargetWeightingModel(DateTimeOffset utcNow)
{
    public string Exchange = string.Empty;
    public string Name;
    public decimal TargetWeighting;
    public DateTimeOffset Timestamp = utcNow;
}