using Microsoft.AspNetCore.Mvc;
using TradingBot.Api.Features.Profile.Model;
using TradingBot.Domain.Service;

namespace TradingBot.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class PortfolioController(IPortfolioService portfolioService,ILogger<PortfolioController> logger) : ControllerBase
{
    private readonly ILogger<PortfolioController> _logger = logger;

    [HttpGet(Name = "GetProfile")]
    public async Task<Portfolio> Get()
    {
        var portfolioObj = await portfolioService.GetPortfoliosAsync();
        
        var portfolio = new Portfolio
        {
            Exchange = portfolioObj.Exchange,
            Positions = portfolioObj.Positions.Select(p => new Position(DateTimeOffset.UtcNow)
            {
                Exchange = p.Exchange,
                Name = p.Name,
                Quantity = p.Quantity,
                CurrentPrice = p.CurrentPrice,
                Timestamp = p.Timestamp
            }).ToList()
        };

        return portfolio;
    }
}