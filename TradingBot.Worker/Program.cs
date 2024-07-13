using Microsoft.EntityFrameworkCore;
using TradingBot.Domain.API.CoinSpotAPI;
using TradingBot.Domain.Provider;
using TradingBot.Domain.Repository.Position;
using TradingBot.Domain.Repository.Ticker;
using TradingBot.Domain.Repository.Trade;
using TradingBot.Domain.Service;
using TradingBot.Infrastructure.Provider;
using TradingBot.Infrastructure.Repository;
using TradingBot.Infrastructure.Repository.DataContext;
using TradingBot.UseCases.Services;
using Refit;
using Serilog;
using Serilog.Enrichers.WithCaller;
using TradingBot.Domain.API.CoinSpotAPI.DelegatingHandler;
using TradingBot.Domain.Repository.Order;
using TradingBot.Domain.Repository.PositionTargetWeighting;
using TradingBot.Domain.Repository.Return;
using TradingBot.Domain.Repository.StrategyLog;
using TradingBot.Domain.Strategy;
using TradingBot.Domain.TimeProvider;
using TradingBot.Usecases.Strategy;
using TradingBot.Worker.workers;

var builder = Host.CreateApplicationBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .Enrich.FromLogContext()
    .Enrich.WithCaller()
    .WriteTo.Console(outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}]  {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

// Register Serilog as the logging provider
builder.Services.AddSerilog(Log.Logger);

builder.Services.AddHostedService<StrategyWorker<ICalculateDailyReturnsStrategy>>();
builder.Services.AddHostedService<StrategyWorker<IGetPriceSnapshotsStrategy>>();
// builder.Services.AddHostedService<StrategyWorker<IRecalculateTargetWeightsStrategy>>();
// builder.Services.AddHostedService<StrategyWorker<IRebalancePortfolioStrategy>>();
//TODO: tidy up how we register the services
// Bind the configuration section to the ApiSettings class
builder.Services.Configure<CoinspotApiSettings>(builder.Configuration.GetSection("CoinspotApiSettings"));
builder.Services
    .AddScoped<ICalculateDailyReturnsStrategy, CalculateDailyReturnsStrategy>()
    .AddScoped<IGetPriceSnapshotsStrategy, GetPriceSnapshotsStrategy>()
    .AddScoped<IRecalculateTargetWeightsStrategy, RecalculateTargetWeightsStrategy>()
    .AddScoped<IRebalancePortfolioStrategy, RebalancePortfolioStrategy>()
    .AddScoped<IExchangeService, CoinSpotExchangeService>()
    .AddScoped<IExchangeProvider, CoinSpotExchangeProvider>()
    .AddScoped<ITradeRepository, TradeRepository>()
    .AddScoped<IPositionSnapshotRepository, PriceSnapshotRepository>()
    .AddScoped<IPositionRepository, PositionRepository>()
    .AddScoped<IPositionTargetWeightingRepository, PositionTargetWeightingRepository>()
    .AddScoped<IStrategyLogRepository, StrategyLogRepository>()
    .AddScoped<IReturnRepository, ReturnRepository>()
    .AddScoped<IOrderRepository, OrderRepository>()
    .AddSingleton<TimeProvider, SystemTimeProvider>()
    .AddDbContext<ApplicationDbContext>(
    options => options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"),b => b.MigrationsAssembly("TradingBot.Worker")))
    .AddScoped<AuthorisationDelegatingHandler>()
    .AddRefitClient<ICoinSpotApi>()
        .ConfigureHttpClient(c => c.BaseAddress = new Uri("https://www.coinspot.com.au"))
        .AddHttpMessageHandler<AuthorisationDelegatingHandler>();
var host = builder.Build();
host.Run();