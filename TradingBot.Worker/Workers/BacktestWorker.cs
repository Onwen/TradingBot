using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
using TradingBot.Domain.Executor;
using TradingBot.Domain.Extension;
using TradingBot.Domain.Model;
using TradingBot.Domain.Service;
using TradingBot.Domain.Strategy;
using TradingBot.Domain.TimeProvider;
using TradingBot.Infrastructure.Repository.DataContext;
using TradingBot.UseCases.Services.Backtest;

namespace TradingBot.Worker.workers;

public class BacktestWorker(
    IServiceProvider serviceProvider,TimeProvider timeProvider, ILogger<SetIntervalWorker> logger) : SetIntervalWorker(timeProvider, logger)
{
    private const string BacktestConfigPath = @"D:\Backtest";
    private const string ResultOutputPath = @"D:\BacktestResults";
    protected override string WorkerName()
    {
        return nameof(BacktestWorker);
    }

    public override Task<bool> ShouldExecute()
    {
        return Task.FromResult(Directory.EnumerateFiles(BacktestConfigPath, "*.json", SearchOption.TopDirectoryOnly).Any());
    }

    public override async Task HandleExecute()
    {
        var file = Directory.EnumerateFiles(BacktestConfigPath, "*.json", SearchOption.TopDirectoryOnly).First();
        var config = JsonSerializer.Deserialize<BacktestStrategyExecutorConfig>(await File.ReadAllTextAsync(file));
        if (config != null) await Backtest(config, config.From, config.To, config.Interval);
        File.Delete(file);
    }

    private async Task Backtest(StrategyExecutorConfig config, DateTimeOffset start, DateTimeOffset end, TimeSpan timestep)
    {
        var time = start;
        var customServiceProvider = CreateCustomServiceProvider(start);
        var scope = customServiceProvider.CreateScope();
        var strategyFactory = scope.ServiceProvider.GetRequiredService<IStrategyFactory>();
        var strategyExecutor = scope.ServiceProvider.GetRequiredService<IStrategyExecutor>();
        var portfolioService = scope.ServiceProvider.GetRequiredService<IPortfolioService>();
        var pricingService = scope.ServiceProvider.GetRequiredService<IPricingService>();
        var backtestingTimeProvider = scope.ServiceProvider.GetRequiredService<TimeProvider>() as StaticTimeProvider;
        if (backtestingTimeProvider == null)
        {
            logger.LogError("TimeProvider is not StaticTimeProvider");
            return;
        }
        //create portfolio with 100 aud position
        var portfolio = new PortfolioModel()
        {
            Positions = new List<PositionModel>
            {
                new(timeProvider.GetUtcNow())
                {
                    Name = "AUD",
                    Quantity = 100,
                    Exchange = "Backtest",
                    CurrentPrice = 1,
                }
            }
        };
        await portfolioService.SavePortfolioAsync(portfolio);
        
        var portfolioReturns = new List<PortfolioModel>() { portfolio };
        while(backtestingTimeProvider.GetUtcNow() < end)
        {
            var signals = await strategyExecutor.GenerateSignals(strategyFactory, timeProvider, config);
            
            var currentPortfolio = await portfolioService.GetPortfoliosAsync();
            var currentPrices = await pricingService.GetPriceSnapshotsAsync();
            var newPortfolio = currentPortfolio.ApplySignals(currentPrices, signals);
            await portfolioService.SavePortfolioAsync(newPortfolio);
            
            portfolioReturns.Add(newPortfolio);
            backtestingTimeProvider.AdvanceTime(timestep);
        }
        
        var json = JsonSerializer.Serialize(portfolioReturns);
        await File.WriteAllTextAsync(Path.Combine(ResultOutputPath, $"portfolio_{start:yyyyMMddHHmmss}_{end:yyyyMMddHHmmss}.json"), json);
    }
    
    private static IServiceProvider CreateCustomServiceProvider(DateTimeOffset start)
    {
        var defaultServices = new ServiceCollection();
        defaultServices.ConfigureServices();
        defaultServices
            .AddDbContext<ApplicationDbContext>(
                options => options.UseNpgsql("Host=localhost;Port=5432;Database=mydatabase;Username=myuser;Password=mypassword",
                    b => b.MigrationsAssembly("TradingBot.Worker")));
        // Create a new service collection for custom services
        var customServices = new ServiceCollection();

        // Copy the default services to the custom service collection
        foreach (var serviceDescriptor in defaultServices)
        {
            customServices.Add(serviceDescriptor);
        }

        // Override specific services
        customServices.AddScoped<IPortfolioService, BacktestPortfolioService>();
        customServices.AddScoped<IPricingService, BacktestPricingService>();
        customServices.AddSingleton<TimeProvider, StaticTimeProvider>(_ => new StaticTimeProvider(start));

        // Build and return the custom service provider
        return customServices.BuildServiceProvider();
    }

    public override int SleepTime()
    {
        // fine next 1 minute interval and calculate time to sleep
        var now = timeProvider.GetUtcNow();
        var nextMinute = new DateTimeOffset(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0, now.Offset);
        var remainder = nextMinute.Minute % 1;
        nextMinute = nextMinute.AddMinutes(1 - remainder);
        var timeToSleep = nextMinute - now;
        return (int)timeToSleep.TotalMilliseconds;
    }
}