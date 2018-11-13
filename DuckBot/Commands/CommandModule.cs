using Discord;
using Discord.Commands;
using DuckBot.Commands.Preconditions;
using DuckBot.Finance;
using DuckBot.UserActions;
using System.Linq;
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
        [Command("help")]
        public async Task HelpAsync()
        {
            await UserHelpHandler.DisplayHelpMenu(Context);
        }

        [Command("setGame")]
        public async Task SetGameAsync([Remainder]string game)
        {
            MainProgram.botCommandPrefix = game;
            await Context.Client.SetGameAsync($"Use {game} help");
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
        [Alias("mT")]
        public async Task MoneyTransferAsync(string targetUser, long amount)
        {

            await UserBankingHandler.TransferCredits(Context, targetUser, amount);
        }

        [Command("debt")]
        public async Task GetBorrowedCreditsAsync()
        {
            long userDebt = UserBankingHandler.GetUserCreditsDebt(Context);

            await Context.Message.Channel.SendMessageAsync($"You owe **{userDebt} Credits**");

        }
        [Command("borrow")]
        public async Task BorrowCreditsAsync(long amount)
        {
            await UserBankingHandler.BorrowCredits(Context, amount);

        }
        [Command("return")]
        public async Task ReturnCreditsAsync(long amount)
        {
            await UserBankingHandler.ReturnCredits(Context, amount);

        }

        //Stocks
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
                    UserStocksHandler.BuyUserStocksAsync(Context, tickerSymbol, amount);
            }
            [Command("sell")]
            [Alias("s")]
            public async Task UserStockSellAsync(string tickerSymbol, long amount)
            {
                 UserStocksHandler.SellUserStocksAsync(Context, tickerSymbol, amount);

            }
            [Command("portfolio")]
            [Alias("p")]
            public async Task UserStockPortfolioAsync()
            {

                UserStocksHandler.DisplayUserStocksAsync(Context);

            }
            [Command("market")]
            [Alias("m")]
            public async Task DisplayMarketStocksAsync()
            {

                UserStocksHandler.DisplayMarketStocksAsync(Context);

            }
        }
    }
}