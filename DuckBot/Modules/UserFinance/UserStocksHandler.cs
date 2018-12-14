using Discord;
using Discord.Commands;
using Discord.WebSocket;
using DuckBot;
using DuckBot.Modules.Finance.CurrencyManager;
using DuckBot.Modules.Finance.ServiceThreads;
using DuckBot_ClassLibrary;
using DuckBot_ClassLibrary.Modules;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DuckBot.Modules.UserFinance
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
        public static async Task BuyUserStocksAsync(SocketCommandContext Context, string tickerSymbol, long buyAmount)
        {
            var marketStockStorage = XmlManager.FromXmlFile<MarketStockStorage>(CoreMethod.GetFileLocation(@"\MarketStocksValue.xml"));

            foreach (var stock in marketStockStorage.MarketStock)
            {
                //Get the user selected stock from storage
                if (stock.StockTicker == tickerSymbol)
                {
                    //Sets buystockExists to true so it won't send a warning saying stock does not exist
                    bool buyStockExists = true;

                    //Get user portfolio
                    var userStocksStorage = XmlManager.FromXmlFile<UserStockStorage>(CoreMethod.GetFileLocation(@"\UserStocks") + @"\" + Context.User.Id.ToString() + ".xml");

                    //Calculate stock price
                    long stockTotalCost = stock.StockPrice * buyAmount;

                    //Return error if stock value is currently at 0
                    if (stock.StockPrice <= 0)
                    {
                        await Context.Message.Channel.SendMessageAsync($"There are no available sellers for stock **{tickerSymbol}**");
                    }
                    //Check if user can buy stock
                    else if (UserCreditsHandler.GetUserCredits(Context) - stockTotalCost < 0)
                    {
                        await Context.Message.Channel.SendMessageAsync($"You do not have enough credits to buy **{buyAmount} {tickerSymbol}** stocks at price of **{UserBankingHandler.CreditCurrencyFormatter(stock.StockPrice)} each** totaling **{UserBankingHandler.CreditCurrencyFormatter(stockTotalCost)} Credits**");
                    }
                    //Check if user is buying 0 or less stocks
                    else if (buyAmount < 1)
                    {
                        await Context.Message.Channel.SendMessageAsync($"You must buy **1 or more** stocks");
                    }
                    else
                    {
                        //Subtract user balance
                        UserCreditsHandler.AddCredits(Context, Convert.ToInt64(-stockTotalCost));


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
                        var userStockStorage = XmlManager.FromXmlFile<UserStockStorage>(CoreMethod.GetFileLocation(@"\UserStocks") + @"\" + Context.User.Id.ToString() + ".xml");
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

                        XmlManager.ToXmlFile(userStockRecord, CoreMethod.GetFileLocation(@"\UserStocks") + @"\" + Context.User.Id.ToString() + ".xml");
                    }

                    //Send warning if stock does not exist
                    if (buyStockExists == false)
                    {
                        await Context.Message.Channel.SendMessageAsync($"Stock **{tickerSymbol}** does not exist in the market");
                    }
                }
            }
        }

        public static async Task SellUserStocksAsync(SocketCommandContext Context, string tickerSymbol, long sellAmount)
        {
            var marketStockStorage = XmlManager.FromXmlFile<MarketStockStorage>(CoreMethod.GetFileLocation(@"\MarketStocksValue.xml"));

            foreach (var stock in marketStockStorage.MarketStock)
            {
                if (stock.StockTicker == tickerSymbol)
                {
                    //Get user portfolio
                    var userStockStorage = XmlManager.FromXmlFile<UserStockStorage>(CoreMethod.GetFileLocation(@"\UserStocks") + @"\" + Context.User.Id.ToString() + ".xml");

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
                                stockTotalWorth = sellAmount * stock.StockPrice;
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
                            UserCreditsHandler.AddCredits(
                                Context,
                                //Subtract tax deductions
                                stockTotalWorth - await UserCreditsTaxHandler.TaxCollectorAsync(
                                    Context, 
                                    stockTotalWorth, 
                                    $"You sold **{sellAmount} {tickerSymbol}** stocks totaling **{UserBankingHandler.CreditCurrencyFormatter(stockTotalWorth)} Credits**"));

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

                            XmlManager.ToXmlFile(userStockRecord, CoreMethod.GetFileLocation(@"\UserStocks") + @"\" + Context.User.Id.ToString() + ".xml");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.StackTrace);
                    }
                }
            }
        }


        public static async Task DisplayUserStocksAsync(SocketCommandContext Context)
        {
            //User stock list
            List<string> userStockTickerList = new List<string>();
            List<string> userStockBuyMarketValueList = new List<string>();

            //Get user portfolio
            var userStockStorage = XmlManager.FromXmlFile<UserStockStorage>(CoreMethod.GetFileLocation(@"\UserStocks") + @"\" + Context.User.Id.ToString() + ".xml");

            var embedBuilder = new EmbedBuilder()
                .WithColor(new Color(40, 144, 175))
                .WithFooter(footer =>
                {
                    footer
                        .WithText("Sent by " + Context.Message.Author.ToString())
                        .WithIconUrl(Context.Message.Author.GetAvatarUrl());
                })
                .WithAuthor(author =>
                {
                    author
                        .WithName("Stock Portfolio - " + Context.Message.Author.ToString())
                        .WithIconUrl("https://melbournechapter.net/images/transparent-folder-2.png");
                });

            //Add stock listing to embed
            foreach (var userStock in userStockStorage.UserStock)
            {
                //get stock market value
                var marketStockStorage = XmlManager.FromXmlFile<MarketStockStorage>(CoreMethod.GetFileLocation(@"\MarketStocksValue.xml"));

                long userStockMarketPrice = 0;
                foreach (var marketStock in marketStockStorage.MarketStock)
                {
                    if (marketStock.StockTicker == userStock.StockTicker)
                    {
                        userStockMarketPrice = marketStock.StockPrice;
                    }
                }

                userStockTickerList.Add($"**{userStock.StockTicker}** - **{userStock.StockAmount}**");
                userStockBuyMarketValueList.Add($"**{userStock.StockBuyPrice}** || **{userStockMarketPrice}**");
            }

            //Join user stock items
            string joinedUserStockTickerList = string.Join(" \n ", userStockTickerList);
            string joinedUserStockBuyMarketValueList = string.Join(" \n ", userStockBuyMarketValueList);

            //Add inline field if user has stocks
            if (!string.IsNullOrEmpty(joinedUserStockTickerList) && !string.IsNullOrEmpty(joinedUserStockBuyMarketValueList))
            {
                embedBuilder.AddInlineField("Stock Ticker - Amount", joinedUserStockTickerList);
                embedBuilder.AddInlineField("Buy price || Market price", joinedUserStockBuyMarketValueList);
            }

            //Send user stock portfolio
            var embed = embedBuilder.Build();

            await Context.Message.Channel.SendMessageAsync(" ", embed: embed).ConfigureAwait(false);
        }

        public static async Task DisplayMarketStocksAsync(SocketCommandContext Context)
        {
            //User stock list
            List<string> stockTickerList = new List<string>();
            List<string> stockBuyMarketValueList = new List<string>();

            //Get market stock value from storage
            var marketStockStorage = XmlManager.FromXmlFile<MarketStockStorage>(CoreMethod.GetFileLocation(@"\MarketStocksValue.xml"));

            //Get is market stock closed
            string IsStockMarketOpen = "Closed";
            if (marketStockStorage.MarketOpen == true)
            {
                IsStockMarketOpen = "Open";
            }
            else
            {
                IsStockMarketOpen = "Closed";
            }

            var embedBuilder = new EmbedBuilder()
                .WithDescription($"Stock market is `{IsStockMarketOpen}`")
                .WithColor(new Color(6, 221, 238))
                .WithFooter(footer =>
                {
                    footer
                        .WithText("Sent by " + Context.Message.Author.ToString())
                        .WithIconUrl(Context.Message.Author.GetAvatarUrl());
                })
                .WithAuthor(author =>
                {
                    author
                        .WithName("Stock Market")
                        .WithIconUrl("https://www.clipartmax.com/png/middle/257-2574787_stock-market-clipart-stock-market-clipart.png");
                });

            foreach (var marketStock in marketStockStorage.MarketStock)
            {
                //Add market stock value to list
                stockTickerList.Add($"**{marketStock.StockTicker}**");
                stockBuyMarketValueList.Add($"**{marketStock.StockPrice}**");
            }

            //Join user stock items
            string joinedUserStockTickerList = string.Join(" \n ", stockTickerList);
            string joinedUserStockBuyMarketValueList = string.Join(" \n ", stockBuyMarketValueList);

            //Add inline field if stock exists
            if (!string.IsNullOrEmpty(joinedUserStockTickerList) && !string.IsNullOrEmpty(joinedUserStockBuyMarketValueList))
            {
                embedBuilder.AddInlineField("Stock Ticker", string.Join(" \n ", stockTickerList));
                embedBuilder.AddInlineField("Market price", string.Join(" \n ", stockBuyMarketValueList));
            }

            //Send user stock portfolio
            var embed = embedBuilder.Build();

            await Context.Message.Channel.SendMessageAsync(" ", embed: embed).ConfigureAwait(false);
        }

        public static async Task DisplayMarketStockInfoAsync(SocketCommandContext Context, string stockTicker)
        {
            var stockResponse = OnlineStockHandler.GetOnlineStockInfo(stockTicker);

            //If user has not entered invalid stock ticker
            if (stockResponse != null)
            {
                //Create embed with stock information
                var embedBuilder = new EmbedBuilder()
                    .WithTitle($"{stockResponse.CompanyName}")
                    .WithDescription($"Sector: {stockResponse.Sector} \n\n {stockResponse.PrimaryExchange}")
                    .WithColor(new Color(192, 192, 192))
                    .WithFooter(footer => {
                        footer
                            .WithText($"Sent by {Context.Message.Author.ToString()}")
                            .WithIconUrl(Context.Message.Author.GetAvatarUrl());
                    })
                    .WithThumbnailUrl(string.Format("https://storage.googleapis.com/iex/api/logos/{0}.png", stockTicker.ToUpper()))
                    .WithAuthor(author => {
                        author
                            .WithName($"Stock Info - {stockResponse.Symbol}")
                            .WithIconUrl("https://d2v9y0dukr6mq2.cloudfront.net/video/thumbnail/ET83clc_gijohrdoy/animation-of-business-or-stock-market-graph-and-arrow-with-alpha-channel-shows-loss-failure-decline-bankrupt_vszr31_cg__F0004.png");
                    })
                    .AddInlineField("Open", $"{stockResponse.Open}")
                    .AddInlineField("Close", $"{stockResponse.Close}")
                    .AddInlineField("High", $"{stockResponse.High} \n {stockResponse.Week52High} | 52 Week")
                    .AddInlineField("Low", $"{stockResponse.Low} \n {stockResponse.Week52Low} | 52 Week")
                    .AddInlineField("Change", $"{stockResponse.Change} \n {stockResponse.ChangePercent}% \n {stockResponse.YtdChange.ToString().Substring(0, stockResponse.YtdChange.ToString().Length - 4)} YTD")
                    .AddInlineField("PE Ratio", $"{stockResponse.PeRatio}");
                var embed = embedBuilder.Build();

                await Context.Message.Channel.SendMessageAsync(" ", embed: embed).ConfigureAwait(false);
            }
            else
            {
                await Context.Message.Channel.SendMessageAsync("Invalid stock ticker, use `.d stock market` for a list of stock tickers");
            }
        }


        private static string GetStockMarketValue(string stockTicker)
        {
            var marketStockStorage = XmlManager.FromXmlFile<MarketStockStorage>(CoreMethod.GetFileLocation(@"\MarketStocksValue.xml"));

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
