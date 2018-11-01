using Discord.Commands;
using Discord.WebSocket;
using DuckBot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DuckBot.Finance
{
    public class UserStocksHandler : ModuleBase<SocketCommandContext>
    {
        internal static string updateTimeContainer = "--Last Update Time--";

        internal static string userStocksStorageLocation = "";
        internal static string marketStocksValueStorageLocation = TaskMethods.GetFileLocation("MarketStocksValue.txt");


        public static void UpdateUserStocks()
        {
            var MarketStocksValueStorage = TaskMethods.ReadFromFileToList("MarketStocksValue.txt");

            DateTime userLastUseDate = DateTime.Now;
            string stockLastUpdateTime = "";
            try
            {
                //Get line containg last update time
                stockLastUpdateTime = MarketStocksValueStorage.First(p => p.Contains(updateTimeContainer));

                //Extract last update date 
                userLastUseDate = DateTime.Parse(stockLastUpdateTime.Substring(updateTimeContainer.Length + 5, stockLastUpdateTime.Length - updateTimeContainer.Length - 5));
            }
            catch (Exception)
            {
            }

            if (userLastUseDate.AddSeconds(5) < DateTime.UtcNow)
            {
                File.WriteAllText(marketStocksValueStorageLocation, "");

                //Write new stock prices
                var sortedMarketStocksValueStorage = MarketStocksValueStorage.Where(p => !p.Contains(updateTimeContainer));
                foreach (var stockItem in sortedMarketStocksValueStorage)
                {
                    //Get ticker + value of old stock
                    int stockTickerLength = 0;

                    stockTickerLength = stockItem.IndexOf(" ", StringComparison.Ordinal);
                    string oldStockPrice = stockItem.Substring(stockTickerLength + 5, stockItem.Length - stockTickerLength - 5);

                    //Get new stock price
                    Random rand = new Random();

                    int stockHeadDirection = rand.Next(2);
                    int stockChangeAmount = rand.Next(1, 8);

                    int stockPriceNew = 0;
                    if (stockHeadDirection == 0)
                    {
                        //Increase
                        stockPriceNew = int.Parse(oldStockPrice) + stockChangeAmount;
                    }
                    else
                    {
                        //Decrease
                        stockPriceNew = int.Parse(oldStockPrice) - stockChangeAmount;

                        if (stockPriceNew < 0)
                        {
                            stockPriceNew = 0;
                        }
                    }

                    //Write new stock price
                    using (System.IO.StreamWriter file = new System.IO.StreamWriter(marketStocksValueStorageLocation, true))
                    {
                        file.WriteLine(stockItem.Substring(0, stockTickerLength) + " >>> " + stockPriceNew);
                    }
                }

                //Write new last update date
                using (System.IO.StreamWriter file = new System.IO.StreamWriter(marketStocksValueStorageLocation, true))
                {
                    file.WriteLine(updateTimeContainer + " >>> " + DateTime.UtcNow);
                }
            }
        }

        public static void BuyUserStocks(SocketCommandContext Context, string tickerSymbol, int stockAmount)
        {
            CheckIfUserHasPortfolio(Context);

            var MarketStocksValueStorage = TaskMethods.ReadFromFileToList("MarketStocksValue.txt");

            MarketStocksValueStorage = MarketStocksValueStorage.Where(p => !p.Contains(updateTimeContainer)).ToList();
            foreach (string stockItem in MarketStocksValueStorage)
            {
                if (stockItem.Substring(0, tickerSymbol.Length) == tickerSymbol)
                {
                    //Get user portfolio
                    var userStocksStorage = TaskMethods.ReadFromFilePathToList(TaskMethods.GetFileLocation(@"\UserStocks") + @"\" + Context.User.ToString() + @"\UserStockPortfolio.txt");

                    //Get ticker
                    int stockTickerLength = 0;

                    stockTickerLength = stockItem.IndexOf(" ", StringComparison.Ordinal);
                    string stockPrice = stockItem.Substring(stockTickerLength + 5, stockItem.Length - stockTickerLength - 5);

                    int stockTotalCost = int.Parse(stockPrice) * stockAmount;

                    //Select buying stock ticker
                    string storedTicker = "";
                    try
                    {
                        storedTicker = userStocksStorage.First(p => p.Contains(stockItem.Substring(0, tickerSymbol.Length))).ToString();
                    }
                    catch (Exception)
                    {
                    }

                    //Check if user already has some of stock currently buying
                    if (!string.IsNullOrEmpty(storedTicker))
                    {
                        //Extract counter behind username in txt file
                        string userStockAmount = storedTicker.Substring(tickerSymbol.Length + 5, storedTicker.Length - tickerSymbol.Length - 5);
                        stockAmount += int.Parse(userStockAmount);
                    }


                    userStocksStorage = userStocksStorage.Where(p => !p.Contains(stockItem.Substring(0, tickerSymbol.Length))).ToList();

                    //Write user stock amount
                    WriteToUserPortfolioFile(
                        userStocksStorage, 
                        stockItem.Substring(0, stockTickerLength) + " >>> " + stockAmount, 
                        TaskMethods.GetFileLocation(@"\UserStocks") + @"\" + Context.User.ToString() + @"\UserStockPortfolio.txt");


                    //Write user Stock buy cost

                }
            }
        }

        private static bool CheckIfUserHasPortfolio(SocketCommandContext Context)
        {
            string userFolderLocation = TaskMethods.GetFileLocation(@"\UserStocks");

            //Create user stock storage directory if not exist
            if (!Directory.Exists(userFolderLocation + @"\" + Context.Message.Author.ToString()))
            {
                System.IO.Directory.CreateDirectory(userFolderLocation + @"\" + Context.Message.Author.ToString());
                return false;
            }
            else
            {
                return true;
            }
        }

        private static void WriteToUserPortfolioFile(List<string> userStocksStorage, string newChangedUserStock, string filePath)
        {
            TaskMethods.WriteListToFile(userStocksStorage, true, filePath);
            TaskMethods.WriteStringToFile(newChangedUserStock, false, filePath);
        }
    }
}
