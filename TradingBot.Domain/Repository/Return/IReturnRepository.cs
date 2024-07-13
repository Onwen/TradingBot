namespace TradingBot.Domain.Repository.Return;

public interface IReturnRepository
{
    // save a list of returnDtos
    Task<bool> SaveReturns(List<ReturnDto> returns);
}