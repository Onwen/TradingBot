using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TradingBot.Worker;
using TradingBot.UseCases;
using TradingBot.Infrastructure.Repository.DataContext;

// Tell Railway this is a background service, not a web service
Environment.SetEnvironmentVariable("RAILWAY_SERVICE_TYPE", "WORKER");

var builder = Host.CreateApplicationBuilder(args);

builder.Services.ConfigureServices();
builder.Services.ConfigureSettings(builder.Configuration);
builder.Services.ConfigureDatabase(builder.Configuration);
builder.Services.ConfigureWorkers();

var host = builder.Build();

// Apply migrations before running the host
try
{
    using (var scope = host.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        dbContext.Database.Migrate(); // This will apply any pending migrations
    }
}
catch (Exception ex)
{
    // Log migration failure but don't crash the app
    var logger = host.Services.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "An error occurred while migrating the database.");
}

host.Run();