using DuckBot_ClassLibrary;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DuckBot.Finance.ServiceThreads
{
    public class MarketStockStorage
    {
        public List<MarketStock> MarketStock { get; set; }
    }
    public class MarketStock
    {
        public string StockTicker { get; set; }
        public long StockPrice { get; set; }
    }

    public class UserMarketStocksUpdater
    {
        public static bool overrideMarketDirection = false;
        public static int marketDirection = 0;

        public static void UpdateMarketStocks()
        {           
            while (MainProgram._stopThreads == false)
            {
                var marketStockStorage = XmlManager.FromXmlFile<MarketStockStorage>(TaskMethods.GetFileLocation(@"\MarketStocksValue.xml"));

                List<MarketStock> updatedMarketStocks = new List<MarketStock>();

                Random rand = new Random();
                foreach (var stock in marketStockStorage.MarketStock)
                {
                    //Calculate new stock price
                    int stockHeadDirection = rand.Next(0, 1);

                    long stockChangeAmount = rand.Next(0, 10000);
                    int stockChangeAmountMultiplier = rand.Next(0, 15);

                    long stockPriceNew = 0;

                    //Override stocks
                    if (overrideMarketDirection == true)
                    {
                        stockHeadDirection = marketDirection;
                    }

                    if (stockHeadDirection == 0)
                    {
                        //Increase

                        stockPriceNew = stock.StockPrice + stockChangeAmount * stockChangeAmountMultiplier;
                    }
                    else
                    {
                        //Decrease
                        if (stock.StockPrice > 0)
                        {
                            stockPriceNew = stock.StockPrice - stockChangeAmount * stockChangeAmountMultiplier;
                        }
                    }

                    //If price went negative, reset to 0
                    if (stockPriceNew < 0)
                    {
                        stockPriceNew = 0;
                    }


                    updatedMarketStocks.Add(new MarketStock {StockTicker = stock.StockTicker, StockPrice = stockPriceNew });
                }

                var marketStock = new MarketStockStorage
                {
                    MarketStock = updatedMarketStocks
                };

                XmlManager.ToXmlFile(marketStock, TaskMethods.GetFileLocation(@"\MarketStocksValue.xml"));

                //Wait 3 seconds
                Thread.Sleep(3000);
            }
        }
    }
}
