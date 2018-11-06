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
    public class UserMarketStocksUpdater
    {
        public static void UpdateMarketStocks()
        {
            while (MainProgram._stopThreads == false)
            {
                try
                {
                    List<string> MarketStocksValueStorage = CoreMethod.ReadFromFileToList("MarketStocksValue.txt");

                    //Get last update time
                    DateTime userLastUseDate = HelperMethod.GetLastUpdateTime(MarketStocksValueStorage);

                    //If 3 seconds has passed, update stocks
                    if (userLastUseDate.AddSeconds(3) < DateTime.UtcNow)
                    {
                        //Console.WriteLine("Stock prices updated - " + DateTime.UtcNow);

                        //Clear file for new stock amounts
                        File.WriteAllText(CoreMethod.GetFileLocation("MarketStocksValue.txt"), "");

                        List<string> sortedMarketStocksValueStorage = MarketStocksValueStorage.Where(p => !p.Contains(HelperMethod.updateTimeContainer)).ToList();
                        foreach (var stockItem in sortedMarketStocksValueStorage)
                        {
                            //Get ticker + value of old stock
                            int stockTickerLength = 0;

                            stockTickerLength = stockItem.IndexOf(" ", StringComparison.Ordinal);
                            string oldStockPrice = stockItem.Substring(stockTickerLength + 5, stockItem.Length - stockTickerLength - 5);

                            //Calculate new stock price
                            Random rand = new Random();

                            int stockHeadDirection = rand.Next(2);
                            int stockChangeAmount = rand.Next(1, rand.Next(2, 2000));
                            int stockChangeAmountMultiplier = rand.Next(0, rand.Next(1, 10));

                            int stockPriceNew = 0;
                            if (stockHeadDirection == 0)
                            {
                                //Increase

                                if (int.Parse(oldStockPrice) <= 0)
                                {
                                    stockPriceNew = int.Parse(oldStockPrice) + stockChangeAmount * stockChangeAmountMultiplier;
                                }
                                else
                                {
                                    try
                                    {
                                        stockPriceNew = int.Parse(oldStockPrice) + (stockChangeAmount + (int.Parse(oldStockPrice) / (int.Parse(oldStockPrice) - rand.Next(1, rand.Next(rand.Next(1, 50), 1000))) * stockChangeAmountMultiplier));
                                    }
                                    catch (Exception)
                                    {
                                        stockPriceNew = int.Parse(oldStockPrice) + stockChangeAmount;
                                    }
                                }
                            }
                            else
                            {
                                //Decrease
                                if (int.Parse(oldStockPrice) <= 0)
                                {

                                }
                                else
                                {
                                    try
                                    {
                                        stockPriceNew = int.Parse(oldStockPrice) - (stockChangeAmount + (int.Parse(oldStockPrice) / (int.Parse(oldStockPrice) - rand.Next(1, rand.Next(rand.Next(1, 50), 1000))) * stockChangeAmountMultiplier));
                                    }
                                    catch (Exception)
                                    {
                                        stockPriceNew = int.Parse(oldStockPrice) - stockChangeAmount;
                                    }
                                }
                            }

                            //If price went negative, reset to 0
                            if (stockPriceNew < 0)
                            {
                                stockPriceNew = 0;
                            }

                            //Log stock prices in console
                            //Console.WriteLine(stockItem.Substring(0, stockTickerLength) + " >>> " + stockPriceNew);

                            //Write new stock price
                            using (System.IO.StreamWriter file = new System.IO.StreamWriter(CoreMethod.GetFileLocation("MarketStocksValue.txt"), true))
                            {
                                file.WriteLine(stockItem.Substring(0, stockTickerLength) + " >>> " + stockPriceNew);
                            }
                        }

                        //Write new stock last update date
                        using (System.IO.StreamWriter file = new System.IO.StreamWriter(CoreMethod.GetFileLocation("MarketStocksValue.txt"), true))
                        {
                            file.WriteLine(HelperMethod.updateTimeContainer + " >>> " + DateTime.UtcNow);
                        }
                    }

                    Thread.Sleep(3000);
                }
                catch (Exception)
                {
                }
            }
        }
    }
}
