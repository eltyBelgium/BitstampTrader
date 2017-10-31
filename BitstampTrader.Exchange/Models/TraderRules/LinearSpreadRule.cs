using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BitstampTrader.Data;
using BitstampTrader.Data.Repositories;
using BitstampTrader.Exchange.Helpers;

namespace BitstampTrader.Exchange.Models.TraderRules
{
    class LinearSpreadRule : ITraderRule
    {
        public async Task<TradingResult> ExecuteAsync(BitstampExchange bitstampExchange)
        {
            // get the latest ticker
            var latestTicker = bitstampExchange.TickerHistory24H.OrderByDescending(t => t.Timestamp).Take(1).First();

            // if the latest ticker is older than 30s then return
            //if (latestTicker.Timestamp < DateTime.Now.AddSeconds(-30)) return new TradingResult { Timestamp = DateTime.Now, Message = "Warning: latest ticker is older then 30s" };


            // don't buy bitcoins if there is already another sell order within range (+- 2%)
            var openSellOrdersDbInRange = new SqlRepository<Order>(new BitstampTraderEntities()).Where(o=> o.SellTimestamp==null && o.BuyPrice > latestTicker.Last * 0.980M && o.BuyPrice < latestTicker.Last *1.020M);
            if (openSellOrdersDbInRange.Count() != 0)
            {
                return new TradingResult {BuyThreshold = Math.Round(latestTicker.Last*0.985M, 2) + " - " + Math.Round(latestTicker.Last*1.015M, 2)};
            }

            // get the minimum and maximums from the last x days
            var minMaxAveragePeriodInDays = int.Parse(ConfigurationManager.AppSettings["MinMaxAveragePeriodInDays"]);
            var tickerTimestampMinus180Days = latestTicker.Timestamp.AddDays(-minMaxAveragePeriodInDays);
            var minMaxLogDb = new SqlRepository<MinMaxLog>(new BitstampTraderEntities()).Where(l => l.Day > tickerTimestampMinus180Days).ToList();
            var min180DayDb = latestTicker.Last;
            var max180DayDb = latestTicker.Last;
            if (minMaxLogDb.Count > 0)
            {
                min180DayDb = minMaxLogDb.OrderBy(l => l.Minimum).First().Minimum;
                max180DayDb = minMaxLogDb.OrderByDescending(l => l.Maximum).First().Maximum;
            }

            var relativePercentage = TradeHelpers.CalculateRelativePercentage(min180DayDb, max180DayDb, latestTicker.Last);
            var maxUsdAmountToBuy = TradeHelpers.CalculateMaxUsdAmountToBuy(bitstampExchange.AccountBalance);
            var btcAmountToBuy = TradeHelpers.CalculateBtcAmountToBuy(maxUsdAmountToBuy, 5M, relativePercentage, latestTicker);

            // buy bitcoins
            await bitstampExchange.BuyLimitOrderAsync(BitstampExchange.BitstampTickerCode.BtcUsd, Math.Round(btcAmountToBuy, 8), Math.Round(latestTicker.Last, 2));

            return new TradingResult { BuyThreshold = Math.Round(latestTicker.Last * 0.985M, 2) + " - " + Math.Round(latestTicker.Last * 1.015M, 2) };
        }
    }
}
