using Microsoft.EntityFrameworkCore;
using TradingBot.Domain.Repository.Position;
using TradingBot.Domain.Repository.Ticker;
using TradingBot.Domain.Repository.Trade;

namespace TradingBot.Infrastructure.Repository.DataContext;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<PositionDto> Positions { get; set; }
    public DbSet<PriceSnapshotDto> Tickers { get; set; }
    public DbSet<TradeDto> Trades { get; set; }
}