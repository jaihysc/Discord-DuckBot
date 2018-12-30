using Discord.Addons.Interactive;
using Discord.Commands;
using DuckBot.Core;
using DuckBot.Modules.Commands.Preconditions;
using DuckBot.Modules.Finance.CurrencyManager;
using DuckBot.Modules.Interaction;
using DuckBot.Modules.UserFinance;
using DuckBot_ClassLibrary;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DuckBot.Modules.Commands.StandardCommands
{
    [Ratelimit(1, 3, Measure.Seconds)]
    [BlacklistedUsersPrecondition]
    [UserStorageCheckerPrecondition]
    public class StandardFinanceCommandModule : InteractiveBase
    {
        [Command("balance", RunMode = RunMode.Async)]
        [Alias("bal")]
        public async Task SlotBalanceAsync()
        {
            long userCredits = UserCreditsHandler.GetUserCredits(Context);

            await Context.Message.Channel.SendMessageAsync(
                $"**{Context.Message.Author.ToString().Substring(0, Context.Message.Author.ToString().Length - 5)}**," +
                $" You have **{UserBankingHandler.CreditCurrencyFormatter(userCredits)} Credits**");
        }
        [Command("daily", RunMode = RunMode.Async)]
        public async Task SlotDailyCreditsAsync()
        {
            await UserGamblingHandler.GiveDailyCreditsAsync(Context);
        }

        [Command("moneyTransfer", RunMode = RunMode.Async)]
        [Alias("mT")]
        public async Task MoneyTransferAsync(string targetUser, long amount)
        {
            await UserCreditsHandler.TransferCredits(Context, targetUser, amount);
        }

        [Command("debt", RunMode = RunMode.Async)]
        public async Task GetBorrowedCreditsAsync()
        {
            long creditsOwed = UserDebtHandler.GetUserCreditsDebt(Context);

            await Context.Message.Channel.SendMessageAsync(UserInteraction.BoldUserName(Context) + $", you owe **{creditsOwed} Credits**");
        }
        [Command("borrow", RunMode = RunMode.Async)]
        public async Task BorrowCreditsAsync(long amount)
        {
            await UserDebtHandler.BorrowCredits(Context, amount);

        }
        [Command("return", RunMode = RunMode.Async)]
        public async Task ReturnCreditsAsync(long amount)
        {
            await UserDebtHandler.ReturnCredits(Context, amount);

        }

        [Command("bankruptcy", RunMode = RunMode.Async)]
        [Ratelimit(1, 48, Measure.Hours)]
        public async Task BankruptcyAsync()
        {
            await ReplyAsync($"Are you sure you want to proceed with bankruptcy {Context.Message.Author.Mention}? \n Type **Y** to confirm");

            //Get response
            var response = await NextMessageAsync();
            if (response.ToString().ToLower() == "y")
            {
                try
                {
                    //Delete user profile
                    UserCreditsHandler.SetCredits(Context, 0);
                    UserDebtHandler.SetDebt(Context, 0);
                    //Delete user stocks
                    File.Delete(CoreMethod.GetFileLocation(@"\UserStocks") + $@"\{Context.Message.Author.Id}.xml");

                    //Send final confirmation
                    await ReplyAsync($"It's all over now {Context.Message.Author.Mention}");
                }
                catch (Exception)
                {
                }

            }
            else
            {
                await ReplyAsync($"Bankruptcy request cancelled {Context.Message.Author.Mention}");
            }


        }

    }
}
