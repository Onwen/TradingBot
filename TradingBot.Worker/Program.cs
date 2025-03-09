using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TradingBot.Worker;
using TradingBot.UseCases;
using TradingBot.Infrastructure.Repository.DataContext; // Add this to reference ApplicationDbContext

var builder = Host.CreateApplicationBuilder(args);

builder.Services.ConfigureServices();
builder.Services.ConfigureSettings(builder.Configuration);
builder.Services.ConfigureDatabase(builder.Configuration);
builder.Services.ConfigureWorkers();

var host = builder.Build();

// Apply migrations before running the host
using (var scope = host.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    dbContext.Database.Migrate(); // This will apply any pending migrations
}

host.Run();