using System.Net;
using TradingBot.Domain.Executor;
using TradingBot.Domain.Model;
using TradingBot.Domain.Service;

namespace TradingBot.Usecases.Executor;

public class TradeExecutor : ITradeExecutor
{
    public async Task<List<MarketOrderModel>> ExecuteTrades(IPortfolioService portfolioService, IPricingService pricingService, IExchangeService exchangeService, List<TradeSignalModel> tradeSignals)
    {
        var sales = tradeSignals.Where(b => b.Action == TradeAction.Sell).ToArray();
        var buys = tradeSignals.Where(b => b.Action == TradeAction.Buy).ToArray();
        
        var tasks = new List<Task<MarketOrderModel>>();
        var portfolio = await portfolioService.GetPortfoliosAsync();
        foreach (var target in sales)
        {
            if (target.Quantity >= 0) continue;
            // determine max can sell and calculate the quantity to sell
            var position = portfolio.Positions?.FirstOrDefault(b => b.Name == target.TickerName);
            if (position == null) continue;
            var quantityToSell = Math.Min(target.Quantity * -1, position.Quantity);
            tasks.Add(exchangeService.MarketSellAsync(target.TickerName, quantityToSell));
        }
        // Wait for all tasks to complete
        var sellResults = await Task.WhenAll(tasks);
        //TODO: handle results
        
        tasks.Clear();
        var priceSnapshots = await pricingService.GetPriceSnapshotsAsync();
        portfolio = await portfolioService.GetPortfoliosAsync();
        var audQuantity = portfolio.Positions?.FirstOrDefault(b => b.Name == "AUD")?.Quantity ?? 0;
        foreach (var target in buys)
        {
            // determine if we have enough funds to buy and calculate the quantity to buy
            var price = priceSnapshots.FirstOrDefault(b => b.Name == target.TickerName)?.Bid ?? 0;
            if (price == 0) continue;
            var maxCanBuy = audQuantity / price;
            var quantityToBuy = Math.Min(target.Quantity, maxCanBuy);
            tasks.Add(exchangeService.MarketBuyAsync(target.TickerName, quantityToBuy));
        }
        var buyResults = await Task.WhenAll(tasks);
        
        return sellResults.Concat(buyResults).ToList();
    }
}