using System;

namespace BitstampTrader.Exchange.Models
{
    public class TradingResult
    {
        public DateTime Timestamp { get; set; }
        public string Message { get; set; }
        public string BuyThreshold { get; set; }
    }
}
