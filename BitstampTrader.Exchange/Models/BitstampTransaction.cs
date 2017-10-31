using System;
using Newtonsoft.Json;

namespace BitstampTrader.Exchange.Models
{
    public class BitstampTransaction
    {
        // Date and time
        [JsonProperty(PropertyName = "datetime")]
        public DateTime Timestamp { get; set; }

        // Transaction ID
        [JsonProperty(PropertyName = "id")]
        public long Id { get; set; }

        // Transaction type: 0 - deposit; 1 - withdrawal; 2 - market trade; 14 - sub account transfer
        [JsonProperty(PropertyName = "type")]
        public int Type { get; set; }

        // USD amount
        [JsonProperty(PropertyName = "usd")]
        public decimal Usd { get; set; }

        // EUR amount
        [JsonProperty(PropertyName = "eur")]
        public decimal Eur { get; set; }

        // BTC amount
        [JsonProperty(PropertyName = "btc")]
        public decimal Btc { get; set; }

        // XRP amount
        [JsonProperty(PropertyName = "xrp")]
        public decimal Xrp { get; set; }

        // Exchange rate
        [JsonProperty(PropertyName = "btc_usd")]
        public decimal ExchangeRateBtcUsd { get; set; }

        // Transaction fee
        [JsonProperty(PropertyName = "fee")]
        public decimal Fee { get; set; }

        // Executed order ID.
        [JsonProperty(PropertyName = "order_id")]
        public long OrderId { get; set; }
    }
}
