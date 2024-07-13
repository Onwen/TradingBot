using Microsoft.Extensions.Logging;
using TradingBot.Domain.Repository.Return;
using TradingBot.Infrastructure.Repository.DataContext;

namespace TradingBot.Infrastructure.Repository;

public class ReturnRepository(ApplicationDbContext dataContext, ILogger<ReturnRepository> logger) : IReturnRepository
{
    public async Task<bool> SaveReturns(List<ReturnDto> returns)
    {
        logger.LogInformation("Saving returns");
        dataContext.Returns.AddRange(returns);
        var saved = await dataContext.SaveChangesAsync();
        logger.LogInformation("Returns saved");
        return saved > 0;
    }
}