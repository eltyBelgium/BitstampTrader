using System;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using BitstampTrader.Data;
using BitstampTrader.Data.Repositories;
using BitstampTrader.Exchange.Helpers;

namespace BitstampTrader.Exchange.Models.TraderRules
{
    class BuyWhenPriceGoesDownAverageRule : ITraderRule
    {
        private readonly int _averagePeriodInMinutes;
        private readonly decimal _averageBuyThresholdInPercents;
        private readonly int _pauseAfterBuyInMinutes;

        public BuyWhenPriceGoesDownAverageRule(int averagePeriodInMinutes, decimal averageBuyThresholdInPercents, int pauseAfterBuyInMinutes)
        {
            _averagePeriodInMinutes = averagePeriodInMinutes;
            _averageBuyThresholdInPercents = averageBuyThresholdInPercents;
            _pauseAfterBuyInMinutes = pauseAfterBuyInMinutes;
        }

        public async Task<TradingResult> ExecuteAsync(BitstampExchange bitstampExchange)
        {
            // get the latest ticker
            var latestTicker = bitstampExchange.TickerHistory24H.OrderByDescending(t => t.Timestamp).Take(1).First();

            // if the latest ticker is older than 30s then return
            if (latestTicker.Timestamp < DateTime.Now.AddSeconds(-30)) return new TradingResult { Timestamp = DateTime.Now, Message = "Warning: latest ticker is older then 30s" };

            // calculate the ticker average of the last x minutes (x = _averagePeriodInMinutes)
            var tickerOfLastXMinutes = bitstampExchange.TickerHistory24H.Where(t => t.Timestamp > DateTime.Now.AddMinutes(-_averagePeriodInMinutes));
            var tickerAverage = tickerOfLastXMinutes.Average(t => t.Last);

            // todo: had to declare these variables outside the if statement, otherwise null execption on linq queries.  Not sure why exactly
            decimal sellPrice;
            DateTime tickerTimestampMinus180Days;

            // is the current price lower than the average minus x percent?
            if (latestTicker.Last < tickerAverage * (1 - _averageBuyThresholdInPercents / 100))
            {
                // don't buy bitcoins if there was already a buy order in the last x minutes
                if (bitstampExchange.LastBuyTimestamp > DateTime.Now.AddMinutes(-_pauseAfterBuyInMinutes)) return new TradingResult { Timestamp = DateTime.Now, Message = "Bitcoins not bought, last buy was from " + bitstampExchange.LastBuyTimestamp };

                // don't buy bitcoins if there is already another sell order within range (+- 5%)
                var sellOrderRangeInPercents = decimal.Parse(ConfigurationManager.AppSettings["SellOrderRangeInPercents"]);
                sellPrice = latestTicker.Last * (1 + sellOrderRangeInPercents / 100);
                var sellOrderExist = bitstampExchange.OpenOrders.Any(o => o.Type == BitstampOrderType.Sell && o.Price > sellPrice * 0.95M && o.Price < sellPrice * 1.05M);
                if (sellOrderExist)
                {
                    return new TradingResult { Timestamp = DateTime.Now, Message = "Bitcoins not bought, already an order in sell range" };
                }

                // get the minimum and maximums from the last x days
                var minMaxAveragePeriodInDays = int.Parse(ConfigurationManager.AppSettings["MinMaxAveragePeriodInDays"]);
                tickerTimestampMinus180Days = latestTicker.Timestamp.AddDays(-minMaxAveragePeriodInDays);
                var minMaxLogDb = new SqlRepository<MinMaxLog>(new BitstampTraderEntities()).Where(l => l.Day > tickerTimestampMinus180Days).ToList();
                var min180DayDb = latestTicker.Last;
                var max180DayDb = latestTicker.Last;
                if (minMaxLogDb.Count > 0)
                {
                    min180DayDb = minMaxLogDb.OrderBy(l => l.Minimum).First().Minimum;
                    max180DayDb = minMaxLogDb.OrderByDescending(l => l.Maximum).First().Maximum;
                }

                //calculate amount to buy
                var relativePercentage = TradeHelpers.CalculateRelativePercentage(min180DayDb, max180DayDb, latestTicker.Last);
                var maxUsdAmountToBuy = TradeHelpers.CalculateMaxUsdAmountToBuy(bitstampExchange.AccountBalance);
                var btcAmountToBuy = TradeHelpers.CalculateBtcAmountToBuy(maxUsdAmountToBuy, 5M, relativePercentage, latestTicker);

                // buy bitcoins
                await bitstampExchange.BuyLimitOrderAsync(BitstampExchange.BitstampTickerCode.BtcUsd, Math.Round(btcAmountToBuy,8), Math.Round(latestTicker.Last, 2));
            }

            return new TradingResult {BuyThreshold = (tickerAverage * (1 - _averageBuyThresholdInPercents / 100)).ToString(CultureInfo.InvariantCulture) };
        }
    }
}
