using System.Threading.Tasks;

namespace BitstampTrader.Exchange.Models.TraderRules
{
    public interface ITraderRule
    {
        Task<TradingResult> ExecuteAsync(BitstampExchange bitstampExchange);
    }
}
