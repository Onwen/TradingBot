using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;
using TradingBot.Domain.Model;
using TradingBot.Domain.Service;
using TradingBot.Domain.Strategy;

namespace TradingBot.Usecases.Strategy;

public class GetPriceSnapshotsStrategy(IExchangeService exchangeService, TimeProvider timeProvider, ILogger<GetPriceSnapshotsStrategy> logger) : IGetPriceSnapshotsStrategy
{
    private const string Strategy = "GetPriceSnapshots";
    private const string GetSnapshots = "Get Snapshots";
    public Task<bool> ShouldExecute()
    {
        return Task.FromResult(true);
    }

    public async Task HandleExecute()
    {
        SetThreadExecutionState(EXECUTION_STATE.ES_CONTINUOUS | EXECUTION_STATE.ES_SYSTEM_REQUIRED | EXECUTION_STATE.ES_AWAYMODE_REQUIRED);
        await exchangeService.GetPriceSnapshotsAsync();
        // log strategy
        await exchangeService.SaveLog(new StrategyLogModel()
            { StrategyName = Strategy, Message = GetSnapshots, Timestamp = timeProvider.GetUtcNow() });
    }

    public int SleepTime()
    {
        // find next 5 minute interval and calculate time to sleep
        var now = timeProvider.GetUtcNow();
        var nextFiveMinute = new DateTimeOffset(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0, now.Offset);
        var remainder = nextFiveMinute.Minute % 5;
        nextFiveMinute = nextFiveMinute.AddMinutes(5 - remainder);
        var timeToSleep = nextFiveMinute - now;
        return (int)timeToSleep.TotalMilliseconds;
    }

    [FlagsAttribute]
    public enum EXECUTION_STATE : uint
    {
        ES_AWAYMODE_REQUIRED = 0x00000040,
        ES_CONTINUOUS = 0x80000000,
        ES_DISPLAY_REQUIRED = 0x00000002,
        ES_SYSTEM_REQUIRED = 0x00000001
        // Legacy flag, should not be used.
        // ES_USER_PRESENT = 0x00000004
    }

    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    static extern EXECUTION_STATE SetThreadExecutionState(EXECUTION_STATE esFlags);
}