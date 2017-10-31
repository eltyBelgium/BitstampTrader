using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BitstampTrader.Exchange.Models;
using Newtonsoft.Json;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using BitstampTrader.Data;
using BitstampTrader.Data.Repositories;
using BitstampTrader.Exchange.Models.TraderRules;

namespace BitstampTrader.Exchange
{
    public class BitstampExchange
    {
        private const string ApiBaseUrl = "https://www.bitstamp.net/api/v2/";
        private BitstampTicker _ticker = new BitstampTicker();
        internal BitstampAccountBalance AccountBalance = new BitstampAccountBalance();
        internal List<BitstampTransaction> LatestTransactions;
        internal List<BitstampOrder> OpenOrders = new List<BitstampOrder>();
        internal readonly List<BitstampTicker> TickerHistory24H = new List<BitstampTicker>();
        private readonly List<ITraderRule> _traderRules;
        internal DateTime LastBuyTimestamp;

        public enum BitstampTickerCode
        {
            BtcUsd, BtcEur, EurUsd, XrpUsd, XrpEur, XrpBtc, LtcUsd, LtcEur, LtcBtc
        }

        public BitstampExchange()
        {
            //_traderRules = new List<ITraderRule> { new BuyWhenPriceGoesDownAverageRule(90, 1.2M, 60) };
            _traderRules = new List<ITraderRule> { new LinearSpreadRule() };
        }

        #region  Api authentication

        private long _nonce = DateTime.Now.Ticks;

        private List<KeyValuePair<string, string>> GetAuthenticationPostData()
        {
            Interlocked.Increment(ref _nonce);

            return new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("key", ConfigurationManager.AppSettings["BitstampApiKey"]),
                new KeyValuePair<string, string>("signature", GetSignature(_nonce, ConfigurationManager.AppSettings["BitstampApiKey"], ConfigurationManager.AppSettings["BitstampApiSecret"], ConfigurationManager.AppSettings["BitstampCustomerId"])),
                new KeyValuePair<string, string>("nonce", _nonce.ToString())
            };
        }

        private string GetSignature(long nonce, string key, string secret, string customerId)
        {
            var msg = $"{nonce}{customerId}{key}";

            return ByteArrayToString(ComputeHash(secret, StringToByteArray(msg))).ToUpper();
        }

        private static string ByteArrayToString(byte[] hash)
        {
            return BitConverter.ToString(hash).Replace("-", "").ToLower();
        }

        private static byte[] StringToByteArray(string str)
        {
            return Encoding.UTF8.GetBytes(str);
        }

        private static byte[] ComputeHash(string key, byte[] data)
        {
            var hashMaker = new HMACSHA256(Encoding.UTF8.GetBytes(key));
            return hashMaker.ComputeHash(data);
        }

        #endregion Api authentication

        #region Api calls

        public async Task<BitstampTicker> GetTickerAsync(BitstampTickerCode tickerCode)
        {
            var result = "";
            try
            {
                //var cts = new CancellationTokenSource();
                //cts.CancelAfter(2500);

                using (var client = new HttpClient())
                using (var response = await client.GetAsync(ApiBaseUrl + "ticker/" + tickerCode.ToString().ToLower()))
                using (var content = response.Content)
                {
                    result = await content.ReadAsStringAsync();
                    _ticker = JsonConvert.DeserializeObject<BitstampTicker>(result);
                    UpdateMinMaxLogs(_ticker.Last);
                    if (tickerCode == BitstampTickerCode.BtcUsd) //todo: make beter storage of tickers, now code expects only BtcUsd ticker
                    {
                        UpdateTickerHistory(_ticker);
                    }
                    
                    return _ticker;
                }
            }
            catch (OperationCanceledException)
            {
                return null;
            }
            catch (Exception e)
            {
                return null;
                //throw new Exception("BitstampExchange.GetTickerAsync() : " + e.Message + Environment.NewLine + result);
            }
        }

        public async Task<List<BitstampOrder>> GetOpenOrdersAsync(string callingMethod)
        {
            var result = "";
            try
            {
                using (var client = new HttpClient())
                using (var response = await client.PostAsync(ApiBaseUrl + "open_orders/btcusd/", new FormUrlEncodedContent(GetAuthenticationPostData())))
                using (var content = response.Content)
                {
                    result = await content.ReadAsStringAsync();
                    OpenOrders = JsonConvert.DeserializeObject<List<BitstampOrder>>(result);
                    return OpenOrders;
                }
            }
            catch (Exception e)
            {
                throw new Exception("BitstampExchange.GetOpenOrdersAsync() : " + e.Message + Environment.NewLine + result + Environment.NewLine + "DEBUG: CALLING METHOD: " + callingMethod);
            }
        }

        public async Task<BitstampAccountBalance> GetAccountBalanceAsync()
        {
            var result = "";
            try
            {
                using (var client = new HttpClient())
                using (var response = await client.PostAsync(ApiBaseUrl + "balance/", new FormUrlEncodedContent(GetAuthenticationPostData())))
                using (var content = response.Content)
                {
                    result = await content.ReadAsStringAsync();
                    AccountBalance = JsonConvert.DeserializeObject<BitstampAccountBalance>(result);
                    return AccountBalance;
                }
            }
            catch (Exception e)
            {
                throw new Exception("BitstampExchange.GetAccountBalanceAsync() : " + e.Message + Environment.NewLine + result);
            }
        }

        public async Task<List<BitstampTransaction>> GetTransactions()
        {
            var result = "";
            try
            {
                // prepare post data
                var postData = GetAuthenticationPostData();
                //postData.Add(new KeyValuePair<string, string>("limit", "20"));

                using (var client = new HttpClient())
                using (var response = await client.PostAsync(ApiBaseUrl + "user_transactions/btcusd/", new FormUrlEncodedContent(postData)))
                using (var content = response.Content)
                {
                    result = await content.ReadAsStringAsync();
                    LatestTransactions = JsonConvert.DeserializeObject<List<BitstampTransaction>>(result.Replace("1E-8", "0.00933300")); //todo REMOVE
                    return LatestTransactions;
                }
            }
            catch (Exception e)
            {
                throw new Exception("BitstampExchange.GetTransactions() : " + e.Message + Environment.NewLine + result);
            }
        }

        public async Task<BitstampOrder> BuyLimitOrderAsync(BitstampTickerCode tickerCode, decimal amount, decimal price)
        {
            var result = "";

            try
            {
                // prepare post data
                var postData = GetAuthenticationPostData();
                postData.Add(new KeyValuePair<string, string>("amount", amount.ToString(CultureInfo.InvariantCulture)));
                postData.Add(new KeyValuePair<string, string>("price", price.ToString(CultureInfo.InvariantCulture)));

                using (var client = new HttpClient())
                using (var response = await client.PostAsync(ApiBaseUrl + "buy/" + tickerCode.ToString().ToLower() + "/", new FormUrlEncodedContent(postData)))
                using (var content = response.Content)
                {
                    result = await content.ReadAsStringAsync();
                    var executedOrder = JsonConvert.DeserializeObject<BitstampOrder>(result);
                    if (executedOrder.Id != 0)
                    {
                        SaveBuyLimitOrderInDb(executedOrder);
                    }
                    else
                    {
                        throw new Exception("Executed buy order id==0!!!!");
                    }
                    return executedOrder;
                }
            }
            catch (Exception e)
            {
                throw new Exception("BitstampExchange.BuyLimitOrderAsync() : " + e.Message + Environment.NewLine + result);
            }
        }

        private async Task<BitstampOrder> SellLimitOrderAsync(decimal amount, decimal price)
        {
            var result = "";

            try
            {
                // prepare post data
                var postData = GetAuthenticationPostData();
                postData.Add(new KeyValuePair<string, string>("amount", amount.ToString(CultureInfo.InvariantCulture)));
                postData.Add(new KeyValuePair<string, string>("price", price.ToString(CultureInfo.InvariantCulture)));

                using (var client = new HttpClient())
                using (var response = await client.PostAsync(ApiBaseUrl + "sell/btcusd/", new FormUrlEncodedContent(postData)))
                using (var content = response.Content)
                {
                    result = await content.ReadAsStringAsync();
                    var executedOrder = JsonConvert.DeserializeObject<BitstampOrder>(result);
                    if (executedOrder.Id != 0)
                    {
                        return executedOrder;
                    }
                    else
                    {
                        throw new Exception("Executed sell order id==0!!!!");
                    }
                }
            }
            catch (Exception e)
            {
                throw new Exception("BitstampExchange.SellLimitOrderAsync() : " + e.Message + Environment.NewLine + result);
            }
        }

        #endregion Api calls

        public async Task<TradingResult> TradeAsync()
        {
            TradingResult result = null;

            foreach (var traderRule in _traderRules)
            {
                result = await traderRule.ExecuteAsync(this);
            }

            return result;
        }

        private void UpdateMinMaxLogs(decimal priceUsd)
        {
            var minMaxLogDb = new SqlRepository<MinMaxLog>(new BitstampTraderEntities());

            // get the record of the current day
            var currentDay = DateTime.Now.Date;
            var dateDb = minMaxLogDb.ToList().FirstOrDefault(l => l.Day == currentDay);

            // if the day record do not exist then add, otherwise update the min and max values if necessary
            if (dateDb == null)
            {
                minMaxLogDb.Add(new MinMaxLog { Day = currentDay, Minimum = priceUsd, Maximum = priceUsd });
            }
            else
            {
                if (dateDb.Minimum > priceUsd) dateDb.Minimum = priceUsd;
                if (dateDb.Maximum < priceUsd) dateDb.Maximum = priceUsd;
            }

            // save changes to database
            minMaxLogDb.Save();
        }

        private void UpdateTickerHistory(BitstampTicker ticker)
        {
            TickerHistory24H.Add(ticker);
            TickerHistory24H.RemoveAll(t => t.Timestamp < DateTime.Now.AddDays(-1));
        }

        private void SaveBuyLimitOrderInDb(BitstampOrder order)
        {
            try
            {
                LastBuyTimestamp = DateTime.Now;

                var bitstampOrdersDb = new SqlRepository<Order>(new BitstampTraderEntities());
                bitstampOrdersDb.Add(new Order { BuyAmount = order.Amount, BuyPrice = order.Price, BuyId = order.Id, Currency = "btc"});
                bitstampOrdersDb.Save();
            }
            catch (Exception e)
            {
                throw new Exception("BitstampExchange.SaveBuyLimitOrderInDb() : " + e.Message);
            }
        }

        private void UpdateSellLimitOrderDb(long buyId, BitstampOrder sellOrder, decimal buyPrice)
        {
            try
            {
                var bitstampOrdersDb = new SqlRepository<Order>(new BitstampTraderEntities());

                // take the db order by buyId
                var orderDb = bitstampOrdersDb.Where(o => o.BuyId == buyId).First();
                orderDb.BuyPrice = buyPrice;
                orderDb.BuyTimestamp = DateTime.Now;
                orderDb.SellAmount = sellOrder.Amount;
                orderDb.SellPrice = sellOrder.Price;
                orderDb.SellId = sellOrder.Id;

                bitstampOrdersDb.Save();
            }
            catch (Exception e)
            {
                throw new Exception("BitstampExchange.UpdateSellLimitOrderDb() : " + e.Message);
            }
        }

        public async Task<string> CheckBitcoinsBoughtAsync()
        {
            var result = "";
            try
            {
                // get all open buy orders from db
                var openBuyOrdersDb = new SqlRepository<Order>(new BitstampTraderEntities()).Where(o => o.BuyTimestamp == null);

                // loop through all open buy orders in db
                foreach (var openBuyOrderDb in openBuyOrdersDb)
                {
                    // can the order be found in the exchange orders?
                    if (OpenOrders.All(o => o.Id != openBuyOrderDb.BuyId))
                    {
                        // order not found in the exchange orders, so the buy order has been executed --> sell the bought bitcoins
                        result = $"{openBuyOrderDb.BuyAmount:N8}BTC bought @{openBuyOrderDb.BuyPrice:F2} (${Math.Round(openBuyOrderDb.BuyAmount * openBuyOrderDb.BuyPrice, 2)})";

                        // update buyprice
                        await GetTransactions();
                        var buyPrice = openBuyOrderDb.BuyPrice;
                        var transaction = LatestTransactions.First(t => t.OrderId == openBuyOrderDb.BuyId);
                        if (transaction.ExchangeRateBtcUsd != openBuyOrderDb.BuyPrice)
                        {
                            result += " - Buy price adjusted to $" + Math.Round(transaction.ExchangeRateBtcUsd, 2);
                            buyPrice = transaction.ExchangeRateBtcUsd;
                        }

                        // place sell order
                        var sellOrderRangeInPercents = decimal.Parse(ConfigurationManager.AppSettings["SellOrderRangeInPercents"]);
                        var orderResult = await SellLimitOrderAsync(openBuyOrderDb.BuyAmount, Math.Round(openBuyOrderDb.BuyPrice * (1 + sellOrderRangeInPercents / 100), 2)); // todo: save bitcoins instead of selling the buy amount

                        await GetOpenOrdersAsync("CheckBitcoinsBoughtAsync()");

                        // todo : temp: check price !!!

                        result += $" - {orderResult.Amount:N8}BTC sell order placed @{orderResult.Price:F2} (${Math.Round(orderResult.Amount * orderResult.Price, 2)})";
                        UpdateSellLimitOrderDb(openBuyOrderDb.BuyId, orderResult, buyPrice);
                    }
                }

                return result;
            }
            catch (Exception e)
            {
                throw new Exception("BitstampExchange.CheckBitcoinsBoughtAsync() : " + e.Message + Environment.NewLine + result);
            }
        }

        public string CheckBitcoinsSold()
        {
            var result = "";

            try
            {
                // get all open sell orders from db
                var bitstampOrdersDb = new SqlRepository<Order>(new BitstampTraderEntities());
                var openSellOrdersDb = bitstampOrdersDb.Where(o => o.SellTimestamp == null && o.BuyTimestamp != null).ToList();

                // loop through all open sell orders in db
                foreach (var orderDb in openSellOrdersDb)
                {
                    // can the sell order be found in the exchange orders?
                    if (OpenOrders.All(o => o.Id != orderDb.SellId))
                    {
                        // sometimes an order is not instant visible in the open orders, if the exchange is busy this could take a few moments
                        if (orderDb.BuyTimestamp > DateTime.Now.AddMinutes(-5)) continue;

                        // order not found in the exchange orders so sell order has been executed
                        var sellPrice = orderDb.SellPrice ?? 0;

                        // update sell timestamp in db
                        orderDb.SellTimestamp = DateTime.Now;
                        bitstampOrdersDb.Update(orderDb);
                        bitstampOrdersDb.Save();

                        result = $"{orderDb.SellAmount:N8}BTC sold @{sellPrice:F2} (${orderDb.SellAmount * sellPrice:F2})";
                    }
                }

                return result;
            }
            catch (Exception e)
            {
                throw new Exception("BitstampExchange.CheckBitcoinsSold() : " + e.Message + Environment.NewLine + result);
            }
        }

        public async Task UpdateFees()
        {
            var isDbUpdated = false;

            try
            {
                // get all buy orders without a fee or value from db
                var repo = new SqlRepository<Order>(new BitstampTraderEntities());
                var bitstampBuyOrdersDb = repo.Where(o => (o.BuyFee == null || o.BuyValue == null) && o.BuyTimestamp != null).ToList();
                var bitstampSellOrdersDb = repo.Where(o => (o.SellFee == null || o.SellValue == null) && o.SellTimestamp != null).ToList();

                if (bitstampBuyOrdersDb.Any() || bitstampSellOrdersDb.Any())
                {
                    // get latest bitstamp transactions
                    await GetTransactions();

                    // loop through selected buy orders from db
                    foreach (var order in bitstampBuyOrdersDb)
                    {
                        // can the buy order found in the latest bitstamp transcaction?
                        var bitstampTransaction = LatestTransactions.FirstOrDefault(t => t.OrderId == order.BuyId);
                        if (bitstampTransaction != null)
                        {
                            // transaction found
                            order.BuyFee = bitstampTransaction.Fee;
                            order.BuyValue = bitstampTransaction.Usd;
                            isDbUpdated = true;
                        }
                        else
                        {
                            throw new Exception("Transaction not found");
                        }
                    }

                    foreach (var order2 in bitstampSellOrdersDb)
                    {
                        // can the sell order found in the latest bitstamp transcaction?
                        var bitstampTransaction = LatestTransactions.FirstOrDefault(t => t.OrderId == order2.SellId);
                        if (bitstampTransaction != null)
                        {
                            // transaction found
                            order2.SellFee = bitstampTransaction.Fee;
                            order2.SellValue = bitstampTransaction.Usd;
                            isDbUpdated = true;
                        }
                        else
                        {
                            //throw new Exception("Transaction not found");
                        }
                    }

                    if (isDbUpdated)
                    {
                        repo.Save();
                    }
                }
            }
            catch (Exception e)
            {
                throw new Exception("BitstampExchange.UpdateFees() : " + e.Message);
            }
        }
    }
}
