using Microsoft.EntityFrameworkCore;
using TradingBot.Domain.Repository.Order;
using TradingBot.Domain.Repository.Position;
using TradingBot.Domain.Repository.PositionTargetWeighting;
using TradingBot.Domain.Repository.Return;
using TradingBot.Domain.Repository.StrategyLog;
using TradingBot.Domain.Repository.Ticker;
using TradingBot.Domain.Repository.Trade;

namespace TradingBot.Infrastructure.Repository.DataContext;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public async Task<List<PriceSnapshotDto>> GetEarliestAskPricesAsync(DateTimeOffset fromDate, DateTimeOffset toDate)
    {
        var query = @"
            WITH RankedTickers AS (
                SELECT
                    ""Id"",
                    ""Exchange"",
                    ""Name"",
                    ""Currency"",
                    ""Bid"",
                    ""Ask"",
                    ""Last"",
                    ""Timestamp"",
                    ROW_NUMBER() OVER (PARTITION BY ""Name"", DATE(""Timestamp"") ORDER BY ""Timestamp"") AS rn
                FROM public.""Tickers""
                WHERE ""Timestamp"" != '-infinity'
                    AND ""Timestamp"" BETWEEN @FromDate AND @ToDate
            )
            SELECT
                ""Id"",
                ""Exchange"",
                ""Name"",
                ""Currency"",
                ""Bid"",
                ""Ask"",
                ""Last"",
                ""Timestamp""
            FROM RankedTickers
            WHERE rn = 1
            ORDER BY ""Name"", ""Timestamp""";

        var parameters = new[]
        {
            new Npgsql.NpgsqlParameter("FromDate", fromDate),
            new Npgsql.NpgsqlParameter("ToDate", toDate)
        };
        
        return await this.Tickers
            .FromSqlRaw(query, parameters)
            .Select(t => new PriceSnapshotDto
            (
                t.Exchange,
                t.Name,
                t.Currency,
                t.Bid,
                t.Ask,
                t.Last,
                t.Timestamp
            ))
            .ToListAsync();
    }
    public DbSet<PositionDto> Positions { get; set; }
    public DbSet<PriceSnapshotDto> Tickers { get; set; }
    public DbSet<TradeDto> Trades { get; set; }
    public DbSet<StrategyLogDto> StrategyLogs { get; set; }
    public DbSet<PositionTargetWeightingDto> PositionTargetWeightings { get; set; }
    public DbSet<ReturnDto> Returns { get; set; }
    public DbSet<OrderDto> Orders { get; set; }
}