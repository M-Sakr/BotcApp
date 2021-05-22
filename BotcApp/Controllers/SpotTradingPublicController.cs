using Binance.Net;
using Binance.Net.Enums;
using Binance.Net.Objects.Spot;
using CryptoExchange.Net.Authentication;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BotcApp.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class SpotTradingPublicController : ControllerBase
    {
        private BinanceClient _client;
        private const string DollarCoin = "USDT";

        /// <summary>
        /// Get orders aggregation
        /// </summary>
        /// <param name="apiKey">Your binance api key, with permission read data only, no trading permission or any other permission needed</param>
        /// <param name="apiSecret">Your binance api secret</param>
        /// <param name="daysBack">loading orders for how many days back, eg: 30 days back which mean the last month</param>
        /// <returns></returns>
        [HttpPost]
        public IEnumerable<CoinAggregation> Post(string apiKey, string apiSecret, int daysBack = 30)
        {
            _client = new BinanceClient(new BinanceClientOptions
            {
                ApiCredentials = new ApiCredentials(
                    apiKey,
                    apiSecret
                    )
            });

            var accountInfo = _client.General.GetAccountInfo();
            if (!accountInfo.Success)
                return new List<CoinAggregation>();

            var balances = accountInfo.Data.Balances.Where(coin => coin.Total > 0).ToList();

            var coinAggregations = new List<CoinAggregation>();

            foreach (var binanceBalance in balances)
            {
                // ignore usdt 
                if (binanceBalance.Asset == DollarCoin)
                    continue;

                var pair = binanceBalance.Asset + DollarCoin;

                var orderList = _client.Spot.Order.GetAllOrders(pair, null, DateTime.Today.AddDays(-daysBack).Date)?.Data?.ToList();
                if (orderList == null)
                    continue;

                var sell = orderList
                    .Where(x => x.Side == OrderSide.Sell && x.Status == OrderStatus.Filled).ToList();
                var buy = orderList
                    .Where(x => x.Side == OrderSide.Buy && x.Status == OrderStatus.Filled).ToList();

                var sellSum = sell.Sum(x => x.Quantity * x.Price);
                var sellCount = sell.Sum(x => x.Quantity);

                var buySum = buy.Sum(x => x.Quantity * x.Price);
                var buyCount = buy.Sum(x => x.Quantity);

                var availableCoins = balances.FirstOrDefault(x => x.Asset == binanceBalance.Asset)?.Total ?? 0;

                var bookPrice = _client.Spot.Market.GetBookPrice(pair);
                if (!bookPrice.Success)
                    continue;

                var currentPrice = bookPrice.Data.BestBidPrice;

                var isLessThanOneDollar = availableCoins * currentPrice < 5;
                if (isLessThanOneDollar)
                    continue;

                var coinAggregation = new CoinAggregation(sellSum, buySum, availableCoins, binanceBalance.Asset, currentPrice, sellCount, buyCount);
                coinAggregations.Add(coinAggregation);
            }

            return coinAggregations.OrderByDescending(x => x.AvailableCoins).ToList();
        }


    }
}
