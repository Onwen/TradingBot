namespace TradingBot.Worker.workers;

public abstract class SetIntervalWorker(TimeProvider timeProvider, ILogger<SetIntervalWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            logger.LogInformation("{worker} running at: {time}", WorkerName(), timeProvider.GetUtcNow());
            
            await Task.Delay(SleepTime(), stoppingToken);

            if (!await ShouldExecute()) continue;
            // log worker executing at
            logger.LogInformation("{worker} executing at: {time}", WorkerName(), timeProvider.GetUtcNow());
            await HandleExecute();
        }
    }
    protected abstract string WorkerName();
    public abstract Task<bool> ShouldExecute();
    public abstract Task HandleExecute();
    public abstract int SleepTime();
}