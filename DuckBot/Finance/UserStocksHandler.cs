﻿using Discord.Commands;
using Discord.WebSocket;
using DuckBot;
using DuckBot.Finance.ServiceThreads;
using DuckBot_ClassLibrary;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DuckBot.Finance
{
    public class UserStockStorage
    {
        public List<UserStock> UserStock { get; set; }
    }
    public class UserStock
    {
        public string StockTicker { get; set; }
        public long StockAmount { get; set; }
        public long StockBuyPrice { get; set; }
    }

    public class UserStocksHandler : ModuleBase<SocketCommandContext>
    {
        internal static string updateTimeContainer = "--Last Update Time--";

        public static async void BuyUserStocksAsync(SocketCommandContext Context, string tickerSymbol, long buyAmount)
        {

            var marketStockStorage = XmlManager.FromXmlFile<MarketStockStorage>(TaskMethods.GetFileLocation(@"\MarketStocksValue.xml"));

            foreach (var stock in marketStockStorage.MarketStock)
            {
                //Get the user selected stock from storage
                if (stock.StockTicker == tickerSymbol)
                {
                    //Sets buystockExists to true so it won't send a warning saying stock does not exist
                    bool buyStockExists = true;

                    //Get user portfolio
                    var userStocksStorage = XmlManager.FromXmlFile<UserStockStorage>(TaskMethods.GetFileLocation(@"\UserStocks") + @"\" + Context.User.Id.ToString() + @"\UserStockPortfolio.xml");

                    //Calculate stock price
                    long stockTotalCost = stock.StockPrice * buyAmount;

                    //Return error if stock value is currently at 0
                    if (stock.StockPrice <= 0)
                    {
                        await Context.Message.Channel.SendMessageAsync($"There are no available sellers for stock {tickerSymbol}");
                    }
                    //Check if user can buy stock
                    else if (UserBankingHandler.GetUserCredits(Context) - stockTotalCost < 0)
                    {
                        await Context.Message.Channel.SendMessageAsync($"You do not have enough credits to buy **{buyAmount} {tickerSymbol}** stocks at price of **{stock.StockPrice} each** totaling **{stockTotalCost} Credits**");
                    }
                    //Check if user is buying 0 or less stocks
                    else if (buyAmount < 1)
                    {
                        await Context.Message.Channel.SendMessageAsync($"You must buy **1 or more** stocks");
                    }
                    else
                    {
                        //Subtract user balance
                        UserBankingHandler.AddCredits(Context, Convert.ToInt32(-stockTotalCost));


                        //Check if user already has some of stock currently buying
                        //If true, Calculates new user stock total
                        long newStockAmount = buyAmount;
                        foreach (var userStock in userStocksStorage.UserStock)
                        {
                            if (userStock.StockTicker == tickerSymbol)
                            {
                                newStockAmount += userStock.StockAmount;
                            }
                        }


                        //Send user receipt
                        await Context.Message.Channel.SendMessageAsync($"You purchased **{buyAmount} {tickerSymbol}** stocks at price of **{stock.StockPrice} each** totaling **{stockTotalCost} Credits**");


                        //Add existing user stocks to list
                        var userStockStorage = XmlManager.FromXmlFile<UserStockStorage>(TaskMethods.GetFileLocation(@"\UserStocks") + @"\" + Context.User.Id.ToString() + @"\UserStockPortfolio.xml");
                        List<UserStock> userStockStorageList = new List<UserStock>();

                        foreach (var userStock in userStockStorage.UserStock)
                        {
                            if (userStock.StockTicker != tickerSymbol)
                            {
                                userStockStorageList.Add(userStock);
                            }
                        }

                        //Add new stock
                        userStockStorageList.Add(new UserStock {StockTicker=tickerSymbol, StockAmount=newStockAmount, StockBuyPrice=stock.StockPrice});

                        //Write user stock amount
                        var userStockRecord = new UserStockStorage
                        {
                            UserStock = userStockStorageList
                        };

                        XmlManager.ToXmlFile(userStockRecord, TaskMethods.GetFileLocation(@"\UserStocks") + @"\" + Context.User.Id.ToString() + @"\UserStockPortfolio.xml");
                    }

                    //Send warning if stock does not exist
                    if (buyStockExists == false)
                    {
                        await Context.Message.Channel.SendMessageAsync($"Stock **{tickerSymbol}** does not exist in the market");
                    }
                }
            }
        }

        public static async void SellUserStocksAsync(SocketCommandContext Context, string tickerSymbol, long sellAmount)
        {
            var marketStockStorage = XmlManager.FromXmlFile<MarketStockStorage>(TaskMethods.GetFileLocation(@"\MarketStocksValue.xml"));

            foreach (var stock in marketStockStorage.MarketStock)
            {
                if (stock.StockTicker == tickerSymbol)
                {
                    //Get user portfolio
                    var userStockStorage = XmlManager.FromXmlFile<UserStockStorage>(TaskMethods.GetFileLocation(@"\UserStocks") + @"\" + Context.User.Id.ToString() + @"\UserStockPortfolio.xml");

                    //Check if user is selling more stocks than they have
                    try
                    {
                        long userStockAmount = 0;
                        long stockTotalWorth = 0;
                        foreach (var userStock in userStockStorage.UserStock)
                        {
                            if (userStock.StockTicker == tickerSymbol)
                            {
                                userStockAmount = userStock.StockAmount;
                                stockTotalWorth = userStock.StockAmount * stock.StockPrice;
                            }
                        }

                        if (userStockAmount - sellAmount < 0)
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
                                stockTotalWorth - await UserBankingHandler.TaxCollectorAsync(
                                    Context, 
                                    stockTotalWorth, 
                                    $"You sold **{sellAmount} {tickerSymbol}** stocks at **{stockTotalWorth} Credits**"));

                            //Send user receipt

                            long newStockAmount = userStockAmount - sellAmount;

                            //Write user stock amount
                            //Add existing user stocks to list
                            List<UserStock> userStockStorageList = new List<UserStock>();

                            foreach (var userStock in userStockStorage.UserStock)
                            {
                                if (userStock.StockTicker != tickerSymbol)
                                {
                                    userStockStorageList.Add(userStock);
                                }
                            }

                            //Add newly sold stock
                            userStockStorageList.Add(new UserStock { StockTicker = tickerSymbol, StockAmount = newStockAmount, StockBuyPrice = stock.StockPrice });

                            //Write user stock amount
                            var userStockRecord = new UserStockStorage
                            {
                                UserStock = userStockStorageList
                            };

                            XmlManager.ToXmlFile(userStockRecord, TaskMethods.GetFileLocation(@"\UserStocks") + @"\" + Context.User.Id.ToString() + @"\UserStockPortfolio.xml");
                        }
                    }
                    catch (Exception ex)
                    {
                        await Context.Message.Channel.SendMessageAsync($"Something has gone wrong!");
                        Console.WriteLine(ex.StackTrace);
                    }
                }
            }
        }


        public static async void DisplayUserStocksAsync(SocketCommandContext Context)
        {
            //User stock list
            List<string> userStockList = new List<string>();

            //Get user portfolio
            var userStockStorage = XmlManager.FromXmlFile<UserStockStorage>(TaskMethods.GetFileLocation(@"\UserStocks") + @"\" + Context.User.Id.ToString() + @"\UserStockPortfolio.xml");


            //Send stock header
            userStockList.Add($"**Stock Ticker - Stock Amount ** || **Buy price** || **Market value**");

            foreach (var userStock in userStockStorage.UserStock)
            {
                //get stock market value
                var marketStockStorage = XmlManager.FromXmlFile<MarketStockStorage>(TaskMethods.GetFileLocation(@"\MarketStocksValue.xml"));

                long userStockMarketPrice = 0;
                foreach (var marketStock in marketStockStorage.MarketStock)
                {
                    if (marketStock.StockTicker == userStock.StockTicker)
                    {
                        userStockMarketPrice = marketStock.StockPrice;
                    }
                }

                userStockList.Add($"**{userStock.StockTicker} - {userStock.StockAmount} ** || **{userStock.StockBuyPrice}** || **{userStockMarketPrice}**");
            }

            //Send user stock amount
            await Context.Message.Channel.SendMessageAsync(string.Join(" \n ", userStockList));
        }

        public static async void DisplayMarketStocksAsync(SocketCommandContext Context)
        {
            //Market stock list
            List<string> marketStockList = new List<string>();

            //Get market stock value from storage
            var marketStockStorage = XmlManager.FromXmlFile<MarketStockStorage>(TaskMethods.GetFileLocation(@"\MarketStocksValue.xml"));

            //Send stock header
            marketStockList.Add($"**Stock Ticker** - **Market value**");
            foreach (var stock in marketStockStorage.MarketStock)
            {
                //Add market stock value to list
                marketStockList.Add($"**{stock.StockTicker}** - **{stock.StockPrice}**");
            }

            //Send market stock to user
            await Context.Message.Channel.SendMessageAsync(string.Join(" \n ", marketStockList));
        }


        private static string GetStockMarketValue(string stockTicker)
        {
            var marketStockStorage = XmlManager.FromXmlFile<MarketStockStorage>(TaskMethods.GetFileLocation(@"\MarketStocksValue.xml"));

            string stockValue = "";
            foreach (var stock in marketStockStorage.MarketStock)
            {
                //Get the user selected stock from storage
                if (stock.StockTicker == stockTicker)
                {
                    stockValue = stock.StockPrice.ToString();
                }
            }

            return stockValue;
        }
    }
}
