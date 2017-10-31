using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitstampTrader.Exchange.Models.TraderRules
{
    class BuyEveryXMinutes : ITraderRule
    {
        public Task<TradingResult> ExecuteAsync(BitstampExchange bitstampExchange)
        {
            throw new NotImplementedException();
        }
    }
}
