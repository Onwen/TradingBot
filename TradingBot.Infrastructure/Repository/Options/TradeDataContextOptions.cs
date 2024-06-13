using Microsoft.Extensions.Options;

namespace TradingBot.Infrastructure.Repository.Options;

public class TradeDataContextOptions
{
    public string ConnectionString { get; set; } = string.Empty;
}