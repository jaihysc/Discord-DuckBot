using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;
using DuckBot_ClassLibrary.Modules;
using DuckBot.Modules.UserActions;
using DuckBot.Modules.Finance.CurrencyManager;
using DuckBot.Core;
using DuckBot_ClassLibrary;

namespace DuckBot.Modules.UserFinance
{
    public class UserGamblingHandler : ModuleBase<SocketCommandContext>
    {
        public static async Task UserGambling(SocketCommandContext Context, SocketMessage message, long gambleAmount)
        {
            //Tell off the user if they are trying to gamble 0 dollars
            if (gambleAmount <= 0)
            {
                await message.Channel.SendMessageAsync("Quack, you have to gamble **1 or more** credits");
            }
            else
            {
                //Get user credits to list
                var userCreditStorage = XmlManager.FromXmlFile<UserStorage>(CoreMethod.GetFileLocation(@"\UserStorage") + @"\" + Context.Message.Author.Id + ".xml");

                //Money subtractor
                if ((userCreditStorage.UserInfo.UserBankingStorage.Credit - gambleAmount) < 0)
                {
                    await message.Channel.SendMessageAsync("You broke quack, you do not have enough credits || **" + UserBankingHandler.CreditCurrencyFormatter(userCreditStorage.UserInfo.UserBankingStorage.Credit) + " Credits**");
                }
                else
                {
                    //Calculate outcome (userCredits - amountGambled + AmountReturned)
                    long returnAmount = CalculateUserGamblingOutcome(gambleAmount);

                    long userReturnAmount = userCreditStorage.UserInfo.UserBankingStorage.Credit - gambleAmount + returnAmount;

                    //Send outcome & calculate taxes
                    //Write credits to file
                    UserCreditsHandler.SetCredits(
                        Context,
                        userReturnAmount - await UserCreditsTaxHandler.TaxCollectorAsync(Context, returnAmount, $"You gambled **{UserBankingHandler.CreditCurrencyFormatter(gambleAmount)} credits** and made **{UserBankingHandler.CreditCurrencyFormatter(returnAmount)} credits**"));
                }
            }
        }

        static Random rand = new Random();
        private static long CalculateUserGamblingOutcome(long gambleAmount)
        {
            long returnAmount = 0;

            double randomNumber = rand.Next(-10000, 1000000);

            //Change this number to change the change of winning
            int randomNumber2 = rand.Next(5);
            //

            //Win
            if (randomNumber2 >= 1)
            {
                if (randomNumber == 999999)
                {
                    returnAmount = gambleAmount * 1000000;
                }
                if (randomNumber >= 970000 && randomNumber <= 999998)
                {
                    returnAmount = gambleAmount * 8;
                }
                if (randomNumber >= 900000 && randomNumber <= 969999)
                {
                    returnAmount = gambleAmount / rand.Next(10);
                }
                if (randomNumber >= 700000 && randomNumber <= 899999)
                {
                    returnAmount = gambleAmount + gambleAmount / 2;
                }
                if (randomNumber >= 650000 && randomNumber <= 699999)
                {
                    returnAmount = gambleAmount * gambleAmount / (gambleAmount - rand.Next(30));
                }
                if (randomNumber >= 650000 && randomNumber <= 649999)
                {
                    returnAmount = gambleAmount + rand.Next(Convert.ToInt32(gambleAmount));
                }
                if (randomNumber >= 500000 && randomNumber <= 599999)
                {
                    returnAmount = gambleAmount + gambleAmount - gambleAmount / 2;
                }
                if (randomNumber >= 470000 && randomNumber <= 499999)
                {
                    returnAmount = returnAmount * returnAmount;
                }
                if (randomNumber >= 450000 && randomNumber <= 469999)
                {
                    returnAmount = returnAmount * returnAmount / rand.Next(5);
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
                    returnAmount = returnAmount - (rand.Next(3) * rand.Next(2, 8));
                }
                if (randomNumber >= 300000 && randomNumber <= 349999)
                {
                    returnAmount = gambleAmount * 2 - (gambleAmount / 3);
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
        public static async Task SlotDailyCreditsAsync(SocketCommandContext Context)
        {
            //Get user storage
            var userLastDailyCreditStorage = XmlManager.FromXmlFile<UserStorage>(CoreMethod.GetFileLocation(@"\UserStorage") + @"\" + Context.Message.Author.Id + ".xml");

            //If 24 hours has passed
            if (userLastDailyCreditStorage.UserInfo.UserDailyLastUseStorage.DateTime.AddHours(24) < DateTime.UtcNow)
            {
                //Add credits
                UserCreditsHandler.AddCredits(Context, long.Parse(SettingsManager.RetrieveFromConfigFile("dailyAmount")));


                //Write last use date
                userLastDailyCreditStorage = XmlManager.FromXmlFile<UserStorage>(CoreMethod.GetFileLocation(@"\UserStorage") + @"\" + Context.Message.Author.Id + ".xml");
                var userRecord = new UserStorage
                {
                    UserId = Context.Message.Author.Id,
                    UserInfo = new UserInfo
                    {
                        UserDailyLastUseStorage = new UserDailyLastUseStorage { DateTime = DateTime.UtcNow },
                        UserBankingStorage = new UserBankingStorage { Credit = userLastDailyCreditStorage.UserInfo.UserBankingStorage.Credit, CreditDebt = userLastDailyCreditStorage.UserInfo.UserBankingStorage.CreditDebt },
                        UserProhibitedWordsStorage = new UserProhibitedWordsStorage { SwearCount = userLastDailyCreditStorage.UserInfo.UserProhibitedWordsStorage.SwearCount }
                    }
                };

                //Write new credits and last redeem date to file
                XmlManager.ToXmlFile(userRecord, CoreMethod.GetFileLocation(@"\UserStorage") + @"\" + Context.Message.Author.Id + ".xml");


                //Send channel message confirmation
                await Context.Message.Channel.SendMessageAsync("You have redeemed your daily **" + UserBankingHandler.CreditCurrencyFormatter(long.Parse(SettingsManager.RetrieveFromConfigFile("dailyAmount"))) + " Credits!**");

            }
            else
            {
                await Context.Message.Channel.SendMessageAsync("You quacker, it has not yet been 24 hours since you last redeemed");
            }

        }
    }
}
