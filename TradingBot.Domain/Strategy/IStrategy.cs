namespace TradingBot.Domain.Strategy;

public interface IStrategy
{
    public Task<bool> ShouldExecute();

    public Task HandleExecute();
    
    public int SleepTime();
}