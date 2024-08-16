using System.Runtime.InteropServices;
using TradingBot.Domain.Service;

namespace TradingBot.Worker.workers;

public class PriceFetcherWorker(
    IServiceScopeFactory scopeFactory,
    TimeProvider timeProvider,
    ILogger<PriceFetcherWorker> logger) : SetIntervalWorker(timeProvider, logger)
{
    private readonly TimeProvider _timeProvider = timeProvider;

    protected override string WorkerName()
    {
        return nameof(PriceFetcherWorker);
    }

    public override Task<bool> ShouldExecute()
    {
        return Task.FromResult(true);
    }

    public override async Task HandleExecute()
    {
        using var scope = scopeFactory.CreateScope();
        var pricingService = scope.ServiceProvider.GetRequiredService<IPricingService>();
        SetThreadExecutionState(EXECUTION_STATE.ES_CONTINUOUS | EXECUTION_STATE.ES_SYSTEM_REQUIRED | EXECUTION_STATE.ES_AWAYMODE_REQUIRED);
        await pricingService.GetPriceSnapshotsAsync();
        logger.LogInformation("{worker} retrieved price snapshots at: {time}", WorkerName(), _timeProvider.GetUtcNow());
    }

    public override int SleepTime()
    {
        // find next 5 minute interval and calculate time to sleep
        var now = _timeProvider.GetUtcNow();
        var nextFiveMinute = new DateTimeOffset(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0, now.Offset);
        var remainder = nextFiveMinute.Minute % 5;
        nextFiveMinute = nextFiveMinute.AddMinutes(5 - remainder);
        var timeToSleep = nextFiveMinute - now;
        return (int)timeToSleep.TotalMilliseconds;
    }

    [Flags]
    private enum EXECUTION_STATE : uint
    {
        ES_AWAYMODE_REQUIRED = 0x00000040,
        ES_CONTINUOUS = 0x80000000,
        ES_DISPLAY_REQUIRED = 0x00000002,
        ES_SYSTEM_REQUIRED = 0x00000001
        // Legacy flag, should not be used.
        // ES_USER_PRESENT = 0x00000004
    }

    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern EXECUTION_STATE SetThreadExecutionState(EXECUTION_STATE esFlags);
}