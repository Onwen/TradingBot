using TradingBot.Domain.Model;

namespace TradingBot.Domain.Extension;

public static class PortfolioModelExtension
{
    public static PortfolioModel ApplySignals(this PortfolioModel portfolioModel, List<PriceSnapshotModel> prices,
        List<TradeSignalModel> signals)
    {
        if (signals == null || signals.Count == 0)
        {
            return portfolioModel;
        }
        
        var updatedModel = new PortfolioModel
        {
            Exchange = portfolioModel.Exchange,
            Positions = new List<PositionModel>()
        };

        var currentCash = portfolioModel.Positions.FirstOrDefault(p => p.Name == "AUD")?.TotalValue ?? 0;
        // sort signals by action, Sell first
        signals = signals.OrderBy(s => s.Action).ToList();
        foreach (var signal in signals)
        {
            var price = prices.FirstOrDefault(p => p.Name == signal.TickerName);
            var position = portfolioModel.Positions.FirstOrDefault(p => p.Name == signal.TickerName);
            if (price == null)
            {
                continue;
            }

            if (position == null)
            {
                position = new PositionModel(price.Timestamp)
                {
                    Name = signal.TickerName,
                    Quantity = 0,
                    Exchange = "Backtest",
                    CurrentPrice = price.Last
                };
            }

            PositionModel updatedPosition = null;
            switch (signal.Action)
            {
                case TradeAction.Buy:
                    (currentCash, updatedPosition) = HandleBuyAndSell(currentCash, position, signal, price);
                    break;
                case TradeAction.Sell:
                    (currentCash, updatedPosition) = HandleBuyAndSell(currentCash, position, signal, price);
                    break;
            }
            if (updatedPosition != null) 
                updatedModel.Positions.Add(updatedPosition);
        }
        // find all positions that have not been updated
        foreach (var position in portfolioModel.Positions)
        {
            //ignore aud
            if (position.Name == "AUD")
            {
                continue;
            }
            if (updatedModel.Positions.All(p => p.Name != position.Name))
            {
                updatedModel.Positions.Add(position);
            }
        }
        
        updatedModel.Positions.Add(new PositionModel(portfolioModel.Positions.FirstOrDefault(p => p.Name == "AUD")?.Timestamp ?? DateTimeOffset.UtcNow)
        {
            Name = "AUD",
            Quantity = currentCash,
            Exchange = "Backtest",
            CurrentPrice = 1
        });
        return updatedModel;
    }
    
    private static (decimal cash, PositionModel updatedPosition) HandleBuyAndSell(decimal currentCash, PositionModel position, TradeSignalModel signal, PriceSnapshotModel price)
    {
        if (signal.Quantity == 0)
        {
            return (currentCash, position);
        }
        var quantity = signal.Quantity;
        var remainingCash = currentCash - (quantity * price.Last);
        if (remainingCash < 0)
        {
            quantity = currentCash / price.Last;
            if (signal.Action == TradeAction.Sell)
            {
                quantity = -quantity;
            }
            //throw new Exception("Insufficient funds");
        }
        var updatedPosition = new PositionModel(position.Timestamp)
        {
            Name = position.Name,
            Quantity = position.Quantity + quantity,
            Exchange = position.Exchange,
            CurrentPrice = price.Last
        };
        
        //safety checks
        if (updatedPosition.Quantity < 0)
        {
            throw new Exception("Invalid quantity");
        }

        return (remainingCash, updatedPosition);
    }
}