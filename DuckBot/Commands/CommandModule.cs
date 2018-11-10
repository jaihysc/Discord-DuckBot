using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using DuckBot;
using DuckBot.Commands.Preconditions;
using DuckBot.Finance;
using DuckBot.UserActions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DuckBot.Commands
{
    [BlacklistedUsersPrecondition]
    [UserStorageCheckerPrecondition]
    public class CommandModule : ModuleBase<SocketCommandContext>
    {
        public static ulong boyRoleId = 380519578138050562;
        public static ulong girlRole2Id = 406545739464835073;

        [Command("help")]
        public async Task HelpAsync([Remainder]string inputCommand)
        {
            await UserHelpHandler.DisplayCommandHelpMenu(Context, inputCommand);
        }
        public async Task HelpAsync()
        {
            await UserHelpHandler.DisplayHelpMenu(Context);
        }

        [Command("setGame")]
        public async Task SetGameAsync([Remainder]string game)
        {
            await Context.Client.SetGameAsync(game);
        }

        //Genders
        [Command("gender boy")]
        public async Task SetGenderMaleAsync()
        {
            var user = Context.User;
            var role = Context.Guild.Roles.FirstOrDefault(x => x.Id == boyRoleId);
            var removeRole = Context.Guild.Roles.FirstOrDefault(x => x.Id == girlRole2Id);

            await (user as IGuildUser).AddRoleAsync(role);
            await (user as IGuildUser).RemoveRoleAsync(removeRole);
        }
        [Command("gender girl")]
        public async Task SetGenderFemaleAsync()
        {
            var user = Context.User;
            var role = Context.Guild.Roles.FirstOrDefault(x => x.Id == girlRole2Id);
            var removeRole = Context.Guild.Roles.FirstOrDefault(x => x.Id == boyRoleId);

            await (user as IGuildUser).AddRoleAsync(role);
            await (user as IGuildUser).RemoveRoleAsync(removeRole);
        }


        //
        //Banking
        [Command("balance")]
        [Alias("bal")]
        public async Task SlotBalanceAsync()
        {
            await UserBankingHandler.DisplayUserCredits(Context);
        }
        [Command("daily")]
        public async Task SlotDailyCreditsAsync()
        {

            await UserGamblingHandler.SlotDailyCreditsAsync(Context);
        }

        [Command("moneyTransfer")]
        public async Task MoneyTransferAsync(string targetUser, int amount)
        {

            await UserBankingHandler.TransferCredits(Context, targetUser, amount);
        }

        [Command("debt")]
        public async Task GetBorrowedCreditsAsync()
        {
            try
            {

                int userDebt = UserBankingHandler.GetUserCreditsDebt(Context);

                await Context.Message.Channel.SendMessageAsync($"You owe **{userDebt} Credits**");
            }
            catch (Exception)
            {
            }

        }
        [Command("borrow")]
        public async Task BorrowCreditsAsync(int amount)
        {
            try
            {

                await UserBankingHandler.BorrowCredits(Context, amount);
            }
            catch (Exception)
            {
            }

        }
        [Command("return")]
        public async Task ReturnCreditsAsync(int amount)
        {
            try
            {

                await UserBankingHandler.ReturnCredits(Context, amount);
            }
            catch (Exception)
            {
            }

        }

        //Stocks
        [Group("game")]
        [Alias("g")]
        public class Game : ModuleBase<SocketCommandContext>
        {
            //Gambling
            [Command("slot")]
            public async Task PlaySlotAsync(int gambleAmount)
            {
                try
                {
                await UserGamblingHandler.UserGambling(Context, Context.Message, gambleAmount);
                }
                catch (Exception)
                {
                }
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
            public async Task UserStockBuyAsync(string tickerSymbol, int amount)
            {
                try
                {
                    UserStocksHandler.BuyUserStocksAsync(Context, tickerSymbol, amount);
                }
                catch (Exception)
                {
                }
            }
            [Command("sell")]
            [Alias("s")]
            public async Task UserStockSellAsync(string tickerSymbol, int amount)
            {
                try
                {
                    UserStocksHandler.SellUserStocksAsync(Context, tickerSymbol, amount);
                }
                catch (Exception)
                {
                }
            }
            [Command("portfolio")]
            [Alias("p")]
            public async Task UserStockPortfolioAsync()
            {
                try
                {
                    UserStocksHandler.DisplayUserStocksAsync(Context);
                }
                catch (Exception)
                {
                }
            }
            [Command("market")]
            [Alias("m")]
            public async Task DisplayMarketStocksAsync()
            {
                try
                {
                    UserStocksHandler.DisplayMarketStocksAsync(Context);
                }
                catch (Exception)
                {
                }
            }
        }
    }
}