using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BitstampTrader.Exchange.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BitstampTrader.Test
{
    [TestClass]
    public class TraderHelperTests
    {
        [TestMethod]
        public void CalculateRelativePercentage()
        {
            // assert
            Assert.AreEqual(1M, TradeHelpers.CalculateRelativePercentage(0, 0, 0));
            Assert.AreEqual(0.5M, TradeHelpers.CalculateRelativePercentage(0, 100, 50));
            Assert.AreEqual(0.25M, TradeHelpers.CalculateRelativePercentage(0, 100, 25));
            Assert.AreEqual(0.5M, TradeHelpers.CalculateRelativePercentage(100, 200, 150));
        }
    }
}
