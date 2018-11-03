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

        //internal static string userStocksStorageLocation = TaskMethods.GetFileLocation(@"\UserStocks");
        //internal static string marketStocksValueStorageLocation = TaskMethods.GetFileLocation("MarketStocksValue.txt");


        /*
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
        */

        public static async void BuyUserStocksAsync(SocketCommandContext Context, string tickerSymbol, int buyAmount)
        {

            var MarketStocksValueStorage = TaskMethods.ReadFromFileToList("MarketStocksValue.txt");

            bool buyStockExists = false;
            MarketStocksValueStorage = MarketStocksValueStorage.Where(p => !p.Contains(updateTimeContainer)).ToList();
            foreach (string stockItem in MarketStocksValueStorage)
            {
                if (stockItem.Substring(0, tickerSymbol.Length) == tickerSymbol)
                {
                    //Sets buystockExists to true so it won't send a warning saying stock does not exist
                    buyStockExists = true;

                    //Get user portfolio
                    var userStocksStorage = TaskMethods.ReadFromFilePathToList(TaskMethods.GetFileLocation(@"\UserStocks") + @"\" + Context.User.Id.ToString() + @"\UserStockPortfolio.txt");

                    //Get target stock ticker
                    int stockTickerLength = 0;

                    stockTickerLength = stockItem.IndexOf(" ", StringComparison.Ordinal);

                    //Get target stock price
                    string stockPrice = stockItem.Substring(stockTickerLength + 5, stockItem.Length - stockTickerLength - 5);

                    int stockTotalCost = int.Parse(stockPrice) * buyAmount;

                    if (UserBankingHandler.GetUserCredits(Context) - stockTotalCost < 0)
                    {
                        await Context.Message.Channel.SendMessageAsync($"You do not have enough credits to buy **{buyAmount} {tickerSymbol}** stocks at price of **{stockPrice} each** totaling **{stockTotalCost} Credits**");
                    }
                    //Check if user is buying 0 or less stocks
                    else if (buyAmount < 1)
                    {
                        await Context.Message.Channel.SendMessageAsync($"You must buy **1 or more** stocks");
                    }
                    else
                    {

                        //Subtract user balance
                        UserBankingHandler.SubtractCredits(Context, stockTotalCost, TaskMethods.GetFileLocation("UserCredits.txt"));

                        //Check if user already has some of stock currently buying
                        //Calculates new user stock total
                        int newStockAmount = buyAmount;
                        foreach (var userStock in userStocksStorage)
                        {
                            if (userStock.Substring(0, tickerSymbol.Length) == tickerSymbol)
                            {
                                //Extract counter behind username in txt file
                                string userStockAmount = userStock.Substring(tickerSymbol.Length + 5, userStock.Length - tickerSymbol.Length - 5);
                                newStockAmount = buyAmount + int.Parse(userStockAmount);
                            }
                        }


                        //Send user receipt
                        await Context.Message.Channel.SendMessageAsync($"You purchased **{buyAmount} {tickerSymbol}** stocks at price of **{stockPrice} each** totaling **{stockTotalCost} Credits**");


                        userStocksStorage = userStocksStorage.Where(p => !p.Contains(stockItem.Substring(0, tickerSymbol.Length))).ToList();

                        //Write user stock amount
                        WriteToUserPortfolioFile(
                            userStocksStorage,
                            stockItem.Substring(0, stockTickerLength) + " >>> " + newStockAmount,
                            TaskMethods.GetFileLocation(@"\UserStocks") + @"\" + Context.User.Id.ToString() + @"\UserStockPortfolio.txt");


                        //Write user Stock buy cost
                        //WIP
                    }
                }
            }
            if (buyStockExists == false)
            {
                await Context.Message.Channel.SendMessageAsync($"Stock **{tickerSymbol}** does not exist in the market");
            }
        }

        public static async void SellUserStocksAsync(SocketCommandContext Context, string tickerSymbol, int sellAmount)
        {
            var MarketStocksValueStorage = TaskMethods.ReadFromFileToList("MarketStocksValue.txt");

            MarketStocksValueStorage = MarketStocksValueStorage.Where(p => !p.Contains(updateTimeContainer)).ToList();
            foreach (string stockItem in MarketStocksValueStorage)
            {
                if (stockItem.Substring(0, tickerSymbol.Length) == tickerSymbol)
                {
                    //Get user portfolio
                    var userStocksStorage = TaskMethods.ReadFromFilePathToList(TaskMethods.GetFileLocation(@"\UserStocks") + @"\" + Context.User.Id.ToString() + @"\UserStockPortfolio.txt");

                    //Get target stock ticker
                    int stockTickerLength = 0;

                    stockTickerLength = stockItem.IndexOf(" ", StringComparison.Ordinal);

                    //Get target stock price
                    string stockPrice = stockItem.Substring(stockTickerLength + 5, stockItem.Length - stockTickerLength - 5);

                    int stockTotalWorth = int.Parse(stockPrice) * sellAmount;

                    string userStockAmount = GetUserStockAmount(tickerSymbol, TaskMethods.GetFileLocation(@"\UserStocks") + @"\" + Context.User.Id.ToString() + @"\UserStockPortfolio.txt");
                    //Check if user is selling more stocks than they have
                    try
                    {
                    if (int.Parse(userStockAmount) - sellAmount < 0)
                    {
                        await Context.Message.Channel.SendMessageAsync($"You do not have enough **{tickerSymbol}** stocks to sell || **{userStockAmount} Stocks**");
                    }
                    //Check if user is selling 0 or less stocks
                    else if (sellAmount < 1)
                    {
                        await Context.Message.Channel.SendMessageAsync($"You must sell **1 or more** stocks");
                    }
                    else
                    {
                        //Add user balance 
                        UserBankingHandler.AddCredits(
                            Context,
                            //Subtract tax deductions
                            await UserBankingHandler.TaxCollectorAsync(
                                Context, 
                                stockTotalWorth, 
                                $"You sold **{sellAmount} {tickerSymbol}** stocks at **{stockTotalWorth} Credits**"),
                            TaskMethods.GetFileLocation("UserCredits.txt"));

                        //Send user receipt
                        //await Context.Message.Channel.SendMessageAsync($"You sold **{sellAmount} {tickerSymbol}** stocks at **{stockTotalWorth} Credits**");

                        userStocksStorage = userStocksStorage.Where(p => !p.Contains(stockItem.Substring(0, tickerSymbol.Length))).ToList();

                        int newStockAmount = int.Parse(userStockAmount) - sellAmount;
                        //Write user stock amount
                        WriteToUserPortfolioFile(
                            userStocksStorage,
                            stockItem.Substring(0, stockTickerLength) + " >>> " + newStockAmount,
                            TaskMethods.GetFileLocation(@"\UserStocks") + @"\" + Context.User.Id.ToString() + @"\UserStockPortfolio.txt");
                    }

                    }
                    catch (Exception)
                    {
                        await Context.Message.Channel.SendMessageAsync($"You do not have any stock **{tickerSymbol}**");
                    }
                }
            }
        }


        public static async void DisplayUserStocksAsync(SocketCommandContext Context)
        {
            //User stock list
            List<string> userStockList = new List<string>();

            //Get user portfolio
            var userStocksStorage = TaskMethods.ReadFromFilePathToList(TaskMethods.GetFileLocation(@"\UserStocks") + @"\" + Context.User.Id.ToString() + @"\UserStockPortfolio.txt");

            //Send stock header
            userStockList.Add($"**Stock Ticker - Stock Amount ** || **Buy price** || **Market value**");
            foreach (string userStock in userStocksStorage)
            {
                //Get target stock ticker
                int stockTickerLength = 0;

                stockTickerLength = userStock.IndexOf(" ", StringComparison.Ordinal);

                //Get target stock buy amount
                string userStockAmount = userStock.Substring(stockTickerLength + 5, userStock.Length - stockTickerLength - 5);

                //Get target stock current worth
                string userStockSellPrice = GetStockMarketValue(userStock.Substring(0, stockTickerLength));

                //Get target stock ticker
                string userStockTicker = userStock.Substring(0, stockTickerLength);

                //Add stock value to list
                userStockList.Add($"**{userStockTicker} - {userStockAmount} ** || **Buy price currently WIP** || **{userStockSellPrice}**");
            }

            //Send user stock amount
            await Context.Message.Channel.SendMessageAsync(string.Join(" \n ", userStockList));
        }

        public static async void DisplayMarketStocksAsync(SocketCommandContext Context)
        {
            //Market stock list
            List<string> marketStockList = new List<string>();

            //Get market stock value from storage
            var MarketStocksValueStorage = TaskMethods.ReadFromFileToList("MarketStocksValue.txt");
            MarketStocksValueStorage = MarketStocksValueStorage.Where(p => !p.Contains(updateTimeContainer)).ToList();

            //Send stock header
            marketStockList.Add($"**Stock Ticker** - **Market value**");
            foreach (string marketStock in MarketStocksValueStorage)
            {
                int stockTickerLength = 0;

                stockTickerLength = marketStock.IndexOf(" ", StringComparison.Ordinal);

                //Get market stock ticker
                string marketStockTicker = marketStock.Substring(0, stockTickerLength);

                //Get market stock value
                string marketStockValue = marketStock.Substring(stockTickerLength + 5, marketStock.Length - stockTickerLength - 5);

                //Add market stock value to list
                marketStockList.Add($"**{marketStockTicker}** - **{marketStockValue}**");
            }

            //Send market stock to user
            await Context.Message.Channel.SendMessageAsync(string.Join(" \n ", marketStockList));
        }

        private static string GetStockMarketValue(string stockTicker)
        {
            var MarketStocksValueStorage = TaskMethods.ReadFromFileToList("MarketStocksValue.txt");

            MarketStocksValueStorage = MarketStocksValueStorage.Where(p => !p.Contains(updateTimeContainer)).ToList();

            string stockValue = "";
            foreach (string stockItem in MarketStocksValueStorage)
            {
                if (stockItem.Length >= stockTicker.Length && 
                    stockItem.Substring(0, stockTicker.Length) == stockTicker)
                {
                    stockValue = stockItem.Substring(stockTicker.Length + 5, stockItem.Length - stockTicker.Length - 5);
                }
                    
            }

            return stockValue;
        }
        private static string GetUserStockAmount(string stockTicker, string filePath)
        {
            var userStocksValueStorage = TaskMethods.ReadFromFilePathToList(filePath);

            string stockValue = "";
            foreach (string stockItem in userStocksValueStorage)
            {
                if (stockItem.Length >= stockTicker.Length &&
                    stockItem.Substring(0, stockTicker.Length) == stockTicker)
                {
                    stockValue = stockItem.Substring(stockTicker.Length + 5, stockItem.Length - stockTicker.Length - 5);
                }

            }

            return stockValue;
        }

        public static bool CheckIfUserHasPortfolio(SocketCommandContext Context)
        {
            string userFolderLocation = TaskMethods.GetFileLocation(@"\UserStocks");

            //Create user stock storage directory if not exist
            if (!Directory.Exists(userFolderLocation + @"\" + Context.Message.Author.Id.ToString()))
            {
                System.IO.Directory.CreateDirectory(userFolderLocation + @"\" + Context.Message.Author.Id.ToString());
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
