using System;
using BitstampTrader.Exchange.Models;
using System.Configuration;

namespace BitstampTrader.Exchange.Helpers
{
    public static class TradeHelpers
    {
        public static decimal CalculateRelativePercentage(decimal periodMinimum, decimal periodMaximum, decimal current)
        {
            // difference between min and max
            var diffBetweenMinMax = periodMaximum - periodMinimum;

            // differenct between current and periodMinimum
            var diffBetweenCurrentAndMinimum = current - periodMinimum;

            decimal relativePercentage = 1;
            if (diffBetweenMinMax > 0)
            {
                relativePercentage = diffBetweenCurrentAndMinimum / diffBetweenMinMax;
            }

            return relativePercentage;
        }

        public static decimal CalculateMaxUsdAmountToBuy(BitstampAccountBalance accountBalance)
        {
            return accountBalance.AvailableUsd / decimal.Parse(ConfigurationManager.AppSettings["AvailableCashDivider"]);
        }

        public static decimal CalculateBtcAmountToBuy(decimal maximumUsd, decimal minimumUsd, decimal offsetPercentage, BitstampTicker lastTicker)
        {
            var maximumUsdToBuy = maximumUsd - (maximumUsd - minimumUsd) * offsetPercentage;

            return Math.Round(maximumUsdToBuy / lastTicker.Last, 6);
        }
    }
}
