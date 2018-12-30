using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;
using DuckBot_ClassLibrary.Modules;
using DuckBot.Modules.UserActions;
using DuckBot.Modules.Finance.CurrencyManager;
using DuckBot.Core;
using DuckBot_ClassLibrary;
using DuckBot.Models;
using DuckBot.Modules.Interaction;

namespace DuckBot.Modules.UserFinance
{
    public class UserGamblingHandler : ModuleBase<SocketCommandContext>
    {
        public static async Task UserGambling(SocketCommandContext context, SocketMessage message, long gambleAmount)
        {
            //Tell off the user if they are trying to gamble 0 dollars
            if (gambleAmount <= 0)
            {
                await message.Channel.SendMessageAsync(UserInteraction.BoldUserName(context) + ", Quack, you have to gamble **1 or more** credits");
            }
            else
            {
                //Get user credits to list
                var userCreditStorage = UserDataManager.GetUserStorage();

                //Money subtractor
                if ((userCreditStorage.UserInfo[context.Message.Author.Id].UserBankingStorage.Credit - gambleAmount) < 0)
                {
                    await message.Channel.SendMessageAsync(UserInteraction.BoldUserName(context) + $", you broke quack, you do not have enough credits || **" + UserBankingHandler.CreditCurrencyFormatter(userCreditStorage.UserInfo[context.Message.Author.Id].UserBankingStorage.Credit) + " Credits**");
                }
                else
                {
                    //Calculate outcome (userCredits - amountGambled + AmountReturned)
                    long returnAmount = CalculateUserGamblingOutcome(gambleAmount);

                    long userReturnAmount = userCreditStorage.UserInfo[context.Message.Author.Id].UserBankingStorage.Credit - gambleAmount + returnAmount;

                    //Send outcome & calculate taxes
                    //Write credits to file
                    UserCreditsHandler.SetCredits(
                        context,
                        userReturnAmount - await UserCreditsTaxHandler.TaxCollectorAsync(context, returnAmount, UserInteraction.BoldUserName(context) + $", you gambled **{UserBankingHandler.CreditCurrencyFormatter(gambleAmount)} credits** and made **{UserBankingHandler.CreditCurrencyFormatter(returnAmount)} credits**"));
                }
            }
        }

        static Random rand = new Random();
        private static long CalculateUserGamblingOutcome(long gambleAmount)
        {
            long returnAmount = 0;

            double randomNumber = rand.Next(-250000, 1000000);

            //Change this number to change the change of winning
            int randomNumber2 = rand.Next(5);
            //

            //Win
            if (randomNumber2 >= 1)
            {
                if (randomNumber == 999999)
                {
                    returnAmount = gambleAmount * 10000000;
                }
                if (randomNumber >= 970000 && randomNumber <= 999998)
                {
                    returnAmount = gambleAmount * 4;
                }
                if (randomNumber >= 900000 && randomNumber <= 969999)
                {
                    returnAmount = gambleAmount * rand.Next(4);
                }
                if (randomNumber >= 700000 && randomNumber <= 899999)
                {
                    returnAmount = gambleAmount + gambleAmount / 2;
                }
                if (randomNumber >= 650000 && randomNumber <= 699999)
                {
                    returnAmount = gambleAmount * gambleAmount + (gambleAmount * rand.Next(5));
                }
                if (randomNumber >= 650000 && randomNumber <= 649999)
                {
                    returnAmount = gambleAmount + rand.Next(Convert.ToInt32(gambleAmount));
                }
                if (randomNumber >= 500000 && randomNumber <= 599999)
                {
                    returnAmount = gambleAmount + gambleAmount + gambleAmount / 2;
                }
                if (randomNumber >= 470000 && randomNumber <= 499999)
                {
                    returnAmount = gambleAmount * gambleAmount / 100;
                }
                if (randomNumber >= 450000 && randomNumber <= 469999)
                {
                    returnAmount = returnAmount * returnAmount / rand.Next(3);
                }
                if (randomNumber >= 430000 && randomNumber <= 449999)
                {
                    returnAmount = returnAmount * rand.Next(10);
                }              
                if (randomNumber >= 400000 && randomNumber <= 429999)
                {
                    returnAmount = returnAmount * rand.Next(1, 5);
                }
                if (randomNumber >= 350000 && randomNumber <= 399999)
                {
                    returnAmount = returnAmount - (rand.Next(3) * rand.Next(1, 8));
                }
                if (randomNumber >= 300000 && randomNumber <= 349999)
                {
                    returnAmount = gambleAmount * 2;
                }
                if (randomNumber >= 0 && randomNumber <= 299999)
                {
                    returnAmount = gambleAmount + gambleAmount;
                }

            }
            else
            {
                returnAmount = 0;
            }

            if (returnAmount < 0) returnAmount = 0;
            return returnAmount;
        }

        //Daily
        public static async Task GiveDailyCreditsAsync(SocketCommandContext context)
        {
            //Get user storage
            var userStorage = UserDataManager.GetUserStorage();

            //If 24 hours has passed
            if (userStorage.UserInfo[context.Message.Author.Id].UserDailyLastUseStorage.DateTime.AddHours(24) < DateTime.UtcNow)
            {
                //Add credits
                UserCreditsHandler.AddCredits(context, long.Parse(SettingsManager.RetrieveFromConfigFile("dailyAmount")));

                //Write last use date
                userStorage.UserInfo[context.Message.Author.Id].UserDailyLastUseStorage.DateTime = DateTime.UtcNow;


                //Write new credits and last redeem date to file
                userStorage = UserDataManager.GetUserStorage();
                userStorage.UserInfo[context.Message.Author.Id].UserDailyLastUseStorage.DateTime = DateTime.UtcNow;
                UserDataManager.WriteUserStorage(userStorage);


                //Send channel message confirmation
                await context.Message.Channel.SendMessageAsync(UserInteraction.BoldUserName(context) + ", you have redeemed your daily **" + UserBankingHandler.CreditCurrencyFormatter(long.Parse(SettingsManager.RetrieveFromConfigFile("dailyAmount"))) + " Credits!**");

            }
            else
            {
                await context.Message.Channel.SendMessageAsync(UserInteraction.BoldUserName(context) + ", you quacker, it has not yet been 24 hours since you last redeemed");
            }

        }
    }
}
