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
        public static void UpdateMarketStocks()
        {
            while (MainProgram._stopThreads == false)
            {
                try
                {
                    var marketStockStorage = XmlManager.FromXmlFile<MarketStockStorage>(TaskMethods.GetFileLocation(@"\MarketStocksValue.xml"));

                    List<MarketStock> updatedMarketStocks = new List<MarketStock>();
                    foreach (var stock in marketStockStorage.MarketStock)
                    {
                        //Calculate new stock price
                        Random rand = new Random();

                        int stockHeadDirection = rand.Next(2);
                        int stockChangeAmount = rand.Next(1, rand.Next(2, 2000));
                        int stockChangeAmountMultiplier = rand.Next(0, rand.Next(1, 10));

                        long stockPriceNew = 0;
                        if (stockHeadDirection == 0)
                        {
                            //Increase

                            if (stock.StockPrice <= 0)
                            {
                                stockPriceNew = stock.StockPrice + stockChangeAmount * stockChangeAmountMultiplier;
                            }
                            else
                            {
                                try
                                {
                                    stockPriceNew = stock.StockPrice + (stockChangeAmount + (stock.StockPrice / (stock.StockPrice - rand.Next(1, rand.Next(rand.Next(1, 50), 1000))) * stockChangeAmountMultiplier));
                                }
                                catch (Exception)
                                {
                                    stockPriceNew = stock.StockPrice + stockChangeAmount;
                                }
                            }
                        }
                        else
                        {
                            //Decrease
                            if (stock.StockPrice <= 0)
                            {
                            }
                            else
                            {
                                try
                                {
                                    stockPriceNew = stock.StockPrice - (stockChangeAmount + (stock.StockPrice / (stock.StockPrice - rand.Next(1, rand.Next(rand.Next(1, 50), 1000))) * stockChangeAmountMultiplier));
                                }
                                catch (Exception)
                                {
                                    stockPriceNew = stock.StockPrice - stockChangeAmount;
                                }
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
                }
                catch (Exception)
                {
                }

                //Wait 3 seconds
                Thread.Sleep(3000);
            }
        }
    }
}
