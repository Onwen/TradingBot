using Microsoft.Extensions.Options;

namespace TradingBot.Infrastructure.Repository.Options;

public class TickerDataContextOptions
{
    public string ConnectionString { get; set; } = string.Empty;
}