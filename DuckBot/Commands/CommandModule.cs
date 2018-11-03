using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using DuckBot;
using DuckBot.Commands.Preconditions;
using DuckBot.Finance;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DuckBot.Commands
{
    [BlacklistedUsersPrecondition]
    public class CommandModule : ModuleBase<SocketCommandContext>
    {
        public static ulong boyRoleId = 380519578138050562;
        public static ulong girlRole2Id = 406545739464835073;

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

        //Banking
        [Command("moneyTransfer")]
        public async Task PlaySlotAsync(string targetUser, int amount)
        {
            UserBankingHandler.CheckIfUserCreditProfileExists(Context);
            await UserBankingHandler.TransferCredits(Context, targetUser, amount);
        }

        //Gambling
        [Command("slot")]
        public async Task PlaySlotAsync(int gambleAmount)
        {
            try
            {
                UserBankingHandler.CheckIfUserCreditProfileExists(Context);
                await UserGamblingHandler.UserGambling(Context, Context.Message, gambleAmount);
            }
            catch (Exception)
            {
            }
        }
        [Command("balance")]
        [Alias("bal")]
        public async Task SlotBalanceAsync()
        {
            UserBankingHandler.CheckIfUserCreditProfileExists(Context);
            await UserBankingHandler.DisplayUserCredits(Context);
        }
        [Command("daily")]
        public async Task SlotDailyCreditsAsync()
        {
            UserBankingHandler.CheckIfUserCreditProfileExists(Context);
            await UserGamblingHandler.SlotDailyCreditsAsync(Context);
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
                UserBankingHandler.CheckIfUserCreditProfileExists(Context);
                UserStocksHandler.CheckIfUserHasPortfolio(Context);

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
                UserBankingHandler.CheckIfUserCreditProfileExists(Context);
                UserStocksHandler.CheckIfUserHasPortfolio(Context);

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
                UserBankingHandler.CheckIfUserCreditProfileExists(Context);
                UserStocksHandler.CheckIfUserHasPortfolio(Context);

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
                UserBankingHandler.CheckIfUserCreditProfileExists(Context);
                UserStocksHandler.CheckIfUserHasPortfolio(Context);

                UserStocksHandler.DisplayMarketStocksAsync(Context);
                }
                catch (Exception)
                {
                }
            }
        }
    }
}