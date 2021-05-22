using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BotcApp
{
    public class CoinAggregation
    {
        public CoinAggregation(decimal sellSum, decimal buySum, decimal available, string asset, decimal currentPrice, decimal sellCount, decimal buyCount)
        {
            Asset = asset;
            CurrentPrice = currentPrice;
            SellSum = sellSum;
            BuySum = buySum;
            AvailableCoins = available;
            SellCount = sellCount;
            BuyCount = buyCount;
            BuySumMinusSellSum = (BuySum - SellSum);
            BuyCountMinusSellCount = BuyCount - SellCount;

            if (available == 0)
            {
                TargetPrice = 0;
            }
            else
            {
                TargetPrice = BuySumMinusSellSum / available;
            }

            AvailableUsdt = AvailableCoins * CurrentPrice;
            Profit = AvailableUsdt - BuySumMinusSellSum;
            var isSellBiggerThanBuy = SellCount > BuyCount;
            if (isSellBiggerThanBuy)
                Message = "The calculation is not very accurate as you sold more than you bought";
        }

        public string Message { get; set; }
        public decimal TargetPrice { get; }
        public decimal Profit { get; }
        public decimal CurrentPrice { get; }
        public decimal AvailableUsdt { get; }
        public decimal BuySumMinusSellSum { get; }
        public decimal BuyCountMinusSellCount { get; }
        public decimal SellSum { get; }
        public decimal BuySum { get; }
        public decimal AvailableCoins { get; }
        public decimal SellCount { get; }
        public decimal BuyCount { get; }
        public decimal Before { get; }
        public string Asset { get; }
    }
}
