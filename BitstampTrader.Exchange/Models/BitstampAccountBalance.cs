using Newtonsoft.Json;

namespace BitstampTrader.Exchange.Models
{
    public class BitstampAccountBalance
    {
        // USD balance
        [JsonProperty(PropertyName = "usd_balance")]
        public decimal BalanceUsd { get; set; }

        // BTC balance
        [JsonProperty(PropertyName = "btc_balance")]
        public decimal BalanceBtc { get; set; }

        // EUR balance
        [JsonProperty(PropertyName = "eur_balance")]
        public decimal BalanceEur { get; set; }

        // XRP balance
        [JsonProperty(PropertyName = "xrp_balance")]
        public decimal BalanceXrp { get; set; }

        // LTC balance
        [JsonProperty(PropertyName = "ltc_balance")]
        public decimal BalanceLtc { get; set; }

        public decimal BalanceTotalUsd { get; set; }

        // USD reserved
        [JsonProperty(PropertyName = "usd_reserved")]
        public decimal ReservedUsd { get; set; }

        // BTC reserved
        [JsonProperty(PropertyName = "btc_reserved")]
        public decimal ReservedBtc { get; set; }

        // EUR reserved
        [JsonProperty(PropertyName = "eur_reserved")]
        public decimal ReservedEur { get; set; }

        // XRP reserved
        [JsonProperty(PropertyName = "xrp_reserved")]
        public decimal ReservedXrp { get; set; }

        // LTC reserved
        [JsonProperty(PropertyName = "ltc_reserved")]
        public decimal ReservedLtc { get; set; }

        public decimal ReservedTotalUsd { get; set; }

        // USD available for trading
        [JsonProperty(PropertyName = "usd_available")]
        public decimal AvailableUsd { get; set; }

        // BTC  available for trading
        [JsonProperty(PropertyName = "btc_available")]
        public decimal AvailableBtc { get; set; }

        // EUR available for trading
        [JsonProperty(PropertyName = "eur_available")]
        public decimal AvailableEur { get; set; }

        // XRP available for trading
        [JsonProperty(PropertyName = "xrp_available")]
        public decimal AvailableXrp { get; set; }

        // LTC available for trading
        [JsonProperty(PropertyName = "ltc_available")]
        public decimal AvailableLtc { get; set; }

        public decimal AvailableTotalUsd { get; set; }

        // Customer BTC/USD trading fee
        [JsonProperty(PropertyName = "btcusd_fee")]
        public decimal FeeBtcUsd { get; set; }

        // Customer BTC/EUR trading fee
        [JsonProperty(PropertyName = "btceur_fee")]
        public decimal FeeBtcEur { get; set; }

        // Customer EUR/USD trading fee
        [JsonProperty(PropertyName = "eurusd_fee")]
        public decimal FeeEurUsd { get; set; }

        // Customer XRP/USD trading fee
        [JsonProperty(PropertyName = "xrpusd_fee")]
        public decimal FeeXrpUsd { get; set; }

        // Customer XRP/EUR trading fee
        [JsonProperty(PropertyName = "xrpeur_fee")]
        public decimal FeeXrpEur { get; set; }

        // Customer XRP/BTC trading fee
        [JsonProperty(PropertyName = "xrpbtc_fee")]
        public decimal FeeXrpBtc { get; set; }

        // Customer LTC/BTC trading fee
        [JsonProperty(PropertyName = "ltcbtc_fee")]
        public decimal FeeLtcBtc { get; set; }

        // Customer LTC/EUR trading fee
        [JsonProperty(PropertyName = "ltceur_fee")]
        public decimal FeeLtcEur { get; set; }

        // Customer LTC/USD trading fee
        [JsonProperty(PropertyName = "ltcusd_fee")]
        public decimal FeeLtcUsd { get; set; }

        // Customer trading fee
        [JsonProperty(PropertyName = "fee")]
        public decimal Fee { get; set; }

        public decimal OfflineBtc { get; set; }
        public decimal OfflineTotalUsd { get; set; }
    }
}
