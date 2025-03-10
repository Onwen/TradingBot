using Microsoft.EntityFrameworkCore;
using Refit;
using Serilog;
using Serilog.Enrichers.WithCaller;
using TradingBot.Domain.API.CoinSpotAPI;
using TradingBot.Domain.API.CoinSpotAPI.DelegatingHandler;
using TradingBot.Domain.Executor;
using TradingBot.Domain.Provider;
using TradingBot.Domain.Repository.Order;
using TradingBot.Domain.Repository.Position;
using TradingBot.Domain.Repository.PositionTargetWeighting;
using TradingBot.Domain.Repository.PriceHistory;
using TradingBot.Domain.Repository.Return;
using TradingBot.Domain.Repository.StrategyLog;
using TradingBot.Domain.Repository.Ticker;
using TradingBot.Domain.Repository.Trade;
using TradingBot.Domain.Service;
using TradingBot.Domain.Strategy;
using TradingBot.Domain.TimeProvider;
using TradingBot.Infrastructure.Provider;
using TradingBot.Infrastructure.Repository;
using TradingBot.Infrastructure.Repository.DataContext;
using TradingBot.Usecases.Executor;
using TradingBot.UseCases.Services;
using TradingBot.Usecases.Strategy;
using TradingBot.Worker.workers;

namespace TradingBot.Worker;

public static class DependencyInjectionExtensions
{

    public static void ConfigureWorkers(this IServiceCollection collection, string workerType)
    {
        switch (workerType)
        {
            case "PriceFetcherWorker":
                collection.AddHostedService<PriceFetcherWorker>();
                break;
            case "DailyReturnCalculatorWorker":
                collection.AddHostedService<DailyReturnCalculatorWorker>();
                break;
            case "StrategyExecutorWorker":
                collection.AddHostedService<StrategyExecutorWorker>();
                break;
            case "DataIngestWorker":
                collection.AddHostedService<DataIngestWorker>();
                break;
            case "BacktestWorker":
                collection.AddHostedService<BacktestWorker>();
                break;
            default:
                throw new ArgumentException($"Invalid worker type [{workerType}]");
        }
    }
    public static void ConfigureSettings(this IServiceCollection collection, IConfigurationManager configurationManager)
    {
        collection.Configure<CoinspotApiSettings>(configurationManager.GetSection("CoinspotApiSettings"));
        collection.Configure<StrategyExecutorConfig>(configurationManager.GetSection("StrategyExecutorConfig"));
    }
    public static void ConfigureDatabase(this IServiceCollection collection, IConfigurationManager configurationManager)
    {
        collection
            .AddDbContext<ApplicationDbContext>(
                options => options.UseNpgsql(configurationManager.GetConnectionString("DefaultConnection"),
                    b => b.MigrationsAssembly("TradingBot.Worker")));
    }
    public static void ConfigureServices(this IServiceCollection collection)
    {
        // Configure Serilog
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .Enrich.FromLogContext()
            .Enrich.WithCaller()
            .WriteTo.Console(outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}]  {Message:lj}{NewLine}{Exception}")
            .CreateLogger();
        // Register Serilog as the logging provider
        collection.AddSerilog(Log.Logger);

        //TODO: tidy up how we register the services
        collection
            .AddScoped<IExchangeService, CoinSpotExchangeService>()
            .AddScoped<IPortfolioService, PortfolioService>()
            .AddScoped<IPricingService, PricingService>()
            .AddScoped<IExchangeProvider, CoinSpotExchangeProvider>()
            .AddScoped<ITradeRepository, TradeRepository>()
            .AddScoped<IPositionSnapshotRepository, PriceSnapshotRepository>()
            .AddScoped<IPositionRepository, PositionRepository>()
            .AddScoped<IPositionTargetWeightingRepository, PositionTargetWeightingRepository>()
            .AddScoped<IStrategyLogRepository, StrategyLogRepository>()
            .AddScoped<IReturnRepository, ReturnRepository>()
            .AddScoped<IPriceHistoryRepository, PriceHistoryRepository>()
            .AddScoped<IStrategyExecutor, StrategyExecutor>()
            .AddScoped<IStrategyFactory, StrategyFactory>()
            .AddScoped<ITradeExecutor, TradeExecutor>()
            .AddScoped<FixedAllocationStrategy>()
            .AddScoped<VARStrategy>()
            .AddScoped<IOrderRepository, OrderRepository>()
            .AddSingleton<TimeProvider, SystemTimeProvider>()
            .AddScoped<AuthorisationDelegatingHandler>()
            .AddRefitClient<ICoinSpotApi>()
            .ConfigureHttpClient(c => c.BaseAddress = new Uri("https://www.coinspot.com.au")) // TODO: should be reading from appsettings.json
            .AddHttpMessageHandler<AuthorisationDelegatingHandler>();
    }
}