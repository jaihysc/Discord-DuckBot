using Discord;
using Discord.Commands;
using DuckBot.Commands.Preconditions;
using DuckBot.Finance;
using DuckBot.Finance.CurrencyManager;
using DuckBot.UserActions;
using System.Linq;
using System.Threading.Tasks;

namespace DuckBot.Commands
{
    [BlacklistedUsersPrecondition]
    [UserStorageCheckerPrecondition]
    public class CommandModule : ModuleBase<SocketCommandContext>
    {
        [Command("help")]
        public async Task HelpAsync([Remainder]string inputCommand = null)
        {
            if (!string.IsNullOrEmpty(inputCommand))
            {
                await UserHelpHandler.DisplayCommandHelpMenu(Context, inputCommand);
            }
            else
            {
                await UserHelpHandler.DisplayHelpMenu(Context);
            }
        }

        //Banking
        [Command("balance")]
        [Alias("bal")]
        public async Task SlotBalanceAsync()
        {
            await UserCreditsHandler.DisplayUserCredits(Context);
        }
        [Command("daily")]
        public async Task SlotDailyCreditsAsync()
        {

            await UserGamblingHandler.SlotDailyCreditsAsync(Context);
        }

        [Command("moneyTransfer")]
        [Alias("mT")]
        public async Task MoneyTransferAsync(string targetUser, long amount)
        {

            await UserCreditsHandler.TransferCredits(Context, targetUser, amount);
        }

        [Command("debt")]
        public async Task GetBorrowedCreditsAsync()
        {
            await UserDebtHandler.DisplayUserCreditsDebt(Context);
        }
        [Command("borrow")]
        public async Task BorrowCreditsAsync(long amount)
        {
            await UserDebtHandler.BorrowCredits(Context, amount);

        }
        [Command("return")]
        public async Task ReturnCreditsAsync(long amount)
        {
            await UserDebtHandler.ReturnCredits(Context, amount);

        }

        //Slot
        [Group("game")]
        [Alias("g")]
        public class Game : ModuleBase<SocketCommandContext>
        {
            //Gambling
            [Command("slot")]
            [Alias("s")]
            public async Task PlaySlotAsync(long gambleAmount)
            {
                await UserGamblingHandler.UserGambling(Context, Context.Message, gambleAmount);

            }
        }

        //Stocks
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