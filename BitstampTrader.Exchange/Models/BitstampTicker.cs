using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace BitstampTrader.Exchange.Models
{
    public class BitstampTicker
    {
        // Last BTC price
        [JsonProperty(PropertyName = "last")]
        public decimal Last { get; set; }

        // Last 24 hours price high
        [JsonProperty(PropertyName = "high")]
        public decimal High { get; set; }

        // Last 24 hours price low
        [JsonProperty(PropertyName = "low")]
        public decimal Low { get; set; }

        // Last 24 hours volume weighted average price
        [JsonProperty(PropertyName = "vwap")]
        public decimal Vwap { get; set; }

        // Last 24 hours volume
        [JsonProperty(PropertyName = "volume")]
        public decimal Volume { get; set; }

        // Highest buy order
        [JsonProperty(PropertyName = "bid")]
        public decimal Bid { get; set; }

        // Lowest sell order
        [JsonProperty(PropertyName = "ask")]
        public decimal Ask { get; set; }

        // Unix timestamp date and time
        [JsonProperty(PropertyName = "timestamp")]
        public long UnixTimestamp { get; set; }

        // First price of the day
        [JsonProperty(PropertyName = "open")]
        public decimal Open { get; set; }

        public DateTime Timestamp
        {
            get
            {
                // Unix timestamp is seconds past epoch
                var dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
                dtDateTime = dtDateTime.AddSeconds(UnixTimestamp).ToLocalTime();
                return dtDateTime;
            }
        }
    }
}
