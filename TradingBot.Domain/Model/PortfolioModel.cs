namespace TradingBot.Domain.Model;

public class PortfolioModel
{
    public string Exchange;
    public List<PositionModel> Positions;
    public decimal TotalValue => Positions.Sum(p => p.Quantity * p.CurrentPrice);
}