using System;

namespace BitstampTrader.Models
{
    public enum LogType { Error, Info }

    public class LogItem
    {
        public DateTime Timestamp { get; set; }
        public LogType Type { get; set; }
        public string Message { get; set; }
    }
}
