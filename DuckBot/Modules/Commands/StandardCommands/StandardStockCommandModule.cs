using Discord.Commands;
using DuckBot.Modules.Commands.Preconditions;
using DuckBot.Modules.UserFinance;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DuckBot.Modules.Commands.StandardCommands
{
    [Ratelimit(1, 4, Measure.Seconds)]
    [BlacklistedUsersPrecondition]
    [UserStorageCheckerPrecondition]
    public class StandardStockCommandModule : ModuleBase<SocketCommandContext>
    {
        [Group("stock")]
        [Alias("s")]
        public class Stock : ModuleBase<SocketCommandContext>
        {
            //Stocks
            [Command("buy")]
            [Alias("b")]
            public async Task UserStockBuyAsync(string tickerSymbol, long amount)
            {
                await UserStocksHandler.BuyUserStocksAsync(Context, tickerSymbol, amount);
            }
            [Command("sell")]
            [Alias("s")]
            public async Task UserStockSellAsync(string tickerSymbol, long amount)
            {
                await UserStocksHandler.SellUserStocksAsync(Context, tickerSymbol, amount);
            }
            [Command("portfolio")]
            [Alias("p")]
            public async Task UserStockPortfolioAsync()
            {
                await UserStocksHandler.DisplayUserStocksAsync(Context);
            }
            [Command("market")]
            [Alias("m")]
            public async Task DisplayMarketStocksAsync()
            {
                await UserStocksHandler.DisplayMarketStocksAsync(Context);
            }
            [Command("market")]
            [Alias("m")]
            public async Task DisplayMarketStockInfoAsync(string stockTicker)
            {
                await UserStocksHandler.DisplayMarketStockInfoAsync(Context, stockTicker);
            }
        }
    }
}
