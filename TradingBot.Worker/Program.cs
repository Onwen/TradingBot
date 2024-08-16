using TradingBot.Worker;
using TradingBot.UseCases;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.ConfigureServices();
builder.Services.ConfigureSettings(builder.Configuration);
builder.Services.ConfigureDatabase(builder.Configuration);
builder.Services.ConfigureWorkers();

var host = builder.Build();
host.Run();