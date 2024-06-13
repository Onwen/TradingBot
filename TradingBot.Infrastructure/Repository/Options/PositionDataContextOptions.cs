using Microsoft.Extensions.Options;

namespace TradingBot.Infrastructure.Repository.Options;

public class PositionDataContextOptions
{
    public string ConnectionString { get; set; } = string.Empty;
}