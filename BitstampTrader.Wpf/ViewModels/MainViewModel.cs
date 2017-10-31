using System;
using System.Configuration;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using BitstampTrader.Exchange;
using BitstampTrader.Exchange.Models;
using GalaSoft.MvvmLight;
using LogItem = BitstampTrader.Models.LogItem;
using LogType = BitstampTrader.Models.LogType;

namespace BitstampTrader.ViewModels
{
    class MainViewModel : ViewModelBase
    {
        private readonly BitstampExchange _bitstampExchange = new BitstampExchange();
        private ObservableCollection<LogItem> _logItems = new ObservableCollection<LogItem>();

        private readonly DispatcherTimer _mainTimer = new DispatcherTimer { Interval = new TimeSpan(0, 0, 10) };


        public ObservableCollection<LogItem> LogItems
        {
            get => _logItems;
            set { Set(() => LogItems, ref _logItems, value); }
        }

        private long _ellapsedExecutionTime;
        public long EllapsedExecutionTime
        {
            get => _ellapsedExecutionTime;
            set { Set(() => EllapsedExecutionTime, ref _ellapsedExecutionTime, value); }
        }

        private string _pageTitle;
        public string PageTitle
        {
            get => _pageTitle;
            set { Set(() => PageTitle, ref _pageTitle, value); }
        }

        private string _buyThreshold;
        public string BuyThreshold
        {
            get => _buyThreshold;
            set { Set(() => BuyThreshold, ref _buyThreshold, value); }
        }

        private BitstampTickers _bitstampTickers = new BitstampTickers();
        public BitstampTickers BitstampTickers
        {
            get => _bitstampTickers;
            set { Set(() => BitstampTickers, ref _bitstampTickers, value); }
        }

        private BitstampAccountBalance _balance;
        public BitstampAccountBalance Balance
        {
            get => _balance;
            set { Set(() => Balance, ref _balance, value); }
        }

        public MainViewModel()
        {
            // prevent xaml designer to trigger constructor
#if DEBUG
            if (DesignerProperties.GetIsInDesignMode(new DependencyObject())) return;
#endif

            _mainTimer.Tick += MainTimerTick;
            _mainTimer.Start();
        }

        private async void MainTimerTick(object state, EventArgs e)
        {
            try
            {
                var watch = new Stopwatch();
                watch.Start();

                var tickerBtcUsd = _bitstampExchange.GetTickerAsync(BitstampExchange.BitstampTickerCode.BtcUsd);
                var tickerBtcEur = _bitstampExchange.GetTickerAsync(BitstampExchange.BitstampTickerCode.BtcEur);
                var tickerEurUsd = _bitstampExchange.GetTickerAsync(BitstampExchange.BitstampTickerCode.EurUsd);
                var tickerXrpUsd = _bitstampExchange.GetTickerAsync(BitstampExchange.BitstampTickerCode.XrpUsd);
                var tickerXrpEur = _bitstampExchange.GetTickerAsync(BitstampExchange.BitstampTickerCode.XrpEur);
                var tickerXrpBtc = _bitstampExchange.GetTickerAsync(BitstampExchange.BitstampTickerCode.XrpBtc);
                var tickerLtcUsd = _bitstampExchange.GetTickerAsync(BitstampExchange.BitstampTickerCode.LtcUsd);
                var tickerLtcEur = _bitstampExchange.GetTickerAsync(BitstampExchange.BitstampTickerCode.LtcEur);
                var tickerLtcBtc = _bitstampExchange.GetTickerAsync(BitstampExchange.BitstampTickerCode.LtcBtc);

                await Task.WhenAll(tickerBtcUsd, tickerBtcEur, tickerEurUsd, tickerXrpUsd, tickerXrpEur, tickerXrpBtc, tickerLtcUsd, tickerLtcEur, tickerLtcBtc);

                var tickersFromBitstamp = new BitstampTickers
                {
                    BtcUsd = tickerBtcUsd.Result,
                    BtcEur = tickerBtcEur.Result,
                    EurUsd = tickerEurUsd.Result,
                    XrpUsd = tickerXrpUsd.Result,
                    XrpEur = tickerXrpEur.Result,
                    XrpBtc = tickerXrpBtc.Result,
                    LtcUsd = tickerLtcUsd.Result,
                    LtcEur = tickerLtcEur.Result,
                    LtcBtc = tickerLtcBtc.Result
                };
                BitstampTickers = tickersFromBitstamp;

                var ticker = await _bitstampExchange.GetTickerAsync(BitstampExchange.BitstampTickerCode.BtcUsd);
                if (ticker == null) return;

                var openOrders = await _bitstampExchange.GetOpenOrdersAsync("MainTimerTick()");

                // test !!! test !!
                if (MoreThan3BuysInLast30minutes(openOrders))
                {
                    _mainTimer.Stop();
                    LogError(new LogItem { Timestamp = DateTime.Now, Type = LogType.Info, Message = "More than 3 buys in the last 30 minutes, execution stopped !!" });
                    return;
                }

                var bitstampBalance = await _bitstampExchange.GetAccountBalanceAsync();
                bitstampBalance.OfflineBtc = decimal.Parse(ConfigurationManager.AppSettings["AvailableBtcInPersonalVault"]);
                bitstampBalance.BalanceBtc += bitstampBalance.OfflineBtc;
                bitstampBalance.AvailableTotalUsd = Math.Round(bitstampBalance.AvailableUsd +
                                                    bitstampBalance.AvailableBtc * tickerBtcUsd.Result.Last +
                                                    bitstampBalance.AvailableEur * tickerEurUsd.Result.Last +
                                                    bitstampBalance.AvailableXrp * tickerXrpUsd.Result.Last +
                                                    bitstampBalance.AvailableLtc * tickerLtcUsd.Result.Last, 2);
                bitstampBalance.ReservedTotalUsd = Math.Round(bitstampBalance.ReservedUsd +
                                                   bitstampBalance.ReservedBtc * tickerBtcUsd.Result.Last +
                                                   bitstampBalance.ReservedEur * tickerEurUsd.Result.Last +
                                                   bitstampBalance.ReservedXrp * tickerXrpUsd.Result.Last +
                                                   bitstampBalance.ReservedLtc * tickerLtcUsd.Result.Last, 2);
                bitstampBalance.OfflineTotalUsd = Math.Round(bitstampBalance.OfflineBtc * tickerBtcUsd.Result.Last, 2);
                bitstampBalance.BalanceTotalUsd = Math.Round(bitstampBalance.BalanceUsd +
                                                  bitstampBalance.BalanceBtc * tickerBtcUsd.Result.Last +
                                                  bitstampBalance.BalanceEur * tickerEurUsd.Result.Last +
                                                  bitstampBalance.BalanceXrp * tickerXrpUsd.Result.Last +
                                                  bitstampBalance.BalanceLtc * tickerLtcUsd.Result.Last, 2);
                Balance = bitstampBalance;

                var resultStr = await _bitstampExchange.CheckBitcoinsBoughtAsync();
                if (resultStr != "") LogError(new LogItem { Timestamp = DateTime.Now, Type = LogType.Info, Message = resultStr });

                resultStr = _bitstampExchange.CheckBitcoinsSold();
                if (resultStr != "") LogError(new LogItem { Timestamp = DateTime.Now, Type = LogType.Info, Message = resultStr });

                var result = await _bitstampExchange.TradeAsync();
                if (!string.IsNullOrEmpty(result?.Message))
                {
                    LogError(new LogItem { Timestamp = DateTime.Now, Type = LogType.Info, Message = result.Message });
                }
                else
                {
                    if (result != null) BuyThreshold = result.BuyThreshold;
                }

                await _bitstampExchange.UpdateFees();

                watch.Stop();
                EllapsedExecutionTime = watch.ElapsedMilliseconds;
                PageTitle = "Main (" + EllapsedExecutionTime + "ms)";
            }
            catch (Exception ex)
            {
                LogError(new LogItem { Timestamp = DateTime.Now, Type = LogType.Error, Message = ex.Message.Trim() });
            }
        }

        private void LogError(LogItem logItem)
        {
            LogItems.Insert(0, logItem);
        }

        private bool MoreThan3BuysInLast30minutes(IEnumerable<BitstampOrder> openOrders)
        {
            var buyOrdersInLast30Minutes = openOrders.Where(o => o.Timestamp > DateTime.Now.AddMinutes(-30));

            return buyOrdersInLast30Minutes.Count() > 2;
        }
    }
}
