using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TradingBot.Domain.Repository.Position;
using TradingBot.Domain.Repository.Trade;
using TradingBot.Infrastructure.Repository.Options;

namespace TradingBot.Infrastructure.Repository.DataContext;

public class PositionDataContext(DbContextOptions<PositionDataContext> options/*IOptions<TradeDataContextOptions> options*/) : DbContext
{
    /*protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // connect to postgres with connection string from app settings
        optionsBuilder.UseNpgsql("postgresql://myuser:mypassword@localhost:5432/mydatabase\n");
    }*/

    public DbSet<PositionDto> Positions { get; set; }
}