using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DuckBot;

namespace DuckBot.Finance
{
    public class UserGamblingHandler : ModuleBase<SocketCommandContext>
    {
        internal static string userDailyLastRedemmedStorageLocation = TaskMethods.GetFileLocation("UserCreditsDailyLastUsed.txt");

        //Slots
        public static async Task UserGambling(SocketCommandContext Context, SocketMessage message, int gambleAmount)
        {
            var userCreditStorage = TaskMethods.ReadFromFileToList("UserCredits.txt");

            //Tell off the user if they are trying to gamble 0 dollars
            if (gambleAmount <= 0)
            {
                await message.Channel.SendMessageAsync("Quack, you have to gamble 1 or more credits");
            }
            else
            {

                //Check if user exists in txt entry
                foreach (var storedUser in userCreditStorage)
                {
                    bool storedUserLengthIsGreaterThanMessageAuthor = false;
                    if (message.Author.Id.ToString().Length <= storedUser.Length) storedUserLengthIsGreaterThanMessageAuthor = true;

                    if (storedUserLengthIsGreaterThanMessageAuthor == true && message.Author.Id.ToString() == storedUser.Substring(0, message.Author.Id.ToString().Length))
                    {
                        //Extract counter behind username in txt file
                        string userCredits = storedUser.Substring(message.Author.Id.ToString().Length + 5, storedUser.Length - message.Author.Id.ToString().Length - 5);

                        //Money subtractor
                        if ((int.Parse(userCredits) - gambleAmount) < 0)
                        {
                            await message.Channel.SendMessageAsync("You broke quack, you do not have enough credits || **" + int.Parse(userCredits) + " Credits**");
                        }
                        else
                        {
                            //Calculate outcome (userCredits - amountGambled + AmountReturned)
                            int returnAmount = CalculateUserGamblingOutcome(gambleAmount);

                            int userReturnAmount = int.Parse(userCredits) - gambleAmount + returnAmount;

                            //Send outcome & calculate taxes
                            //Write credits to file
                            UserBankingHandler.SetCredits(
                                Context,
                                userReturnAmount - await UserBankingHandler.TaxCollectorAsync(Context, returnAmount, $"You gambled **{gambleAmount} credits** and made **{returnAmount} credits**"));
                        }
                    }
                }
            }
        }

        private static int CalculateUserGamblingOutcome(int gambleAmount)
        {
            Random rand = new Random();
            int returnAmount = 0;

            int randomNumber = rand.Next(1000000);

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
                    returnAmount = gambleAmount + rand.Next(gambleAmount);
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
            //DAILY AMOUNT
            int dailyAmount = 5000;
            //

            var userLastDailyCreditStorage = TaskMethods.ReadFromFileToList("UserCreditsDailyLastUsed.txt");

            foreach (var storedUser in userLastDailyCreditStorage)
            {
                bool storedUserLengthIsGreaterThanMessageAuthor = false;
                if (Context.Message.Author.Id.ToString().Length <= storedUser.Length) storedUserLengthIsGreaterThanMessageAuthor = true;

                if (storedUserLengthIsGreaterThanMessageAuthor == true && Context.Message.Author.Id.ToString() == storedUser.Substring(0, Context.Message.Author.Id.ToString().Length))
                {
                    //Extract counter behind username in txt file
                    DateTime userLastUseDate = DateTime.Parse(storedUser.Substring(Context.Message.Author.Id.ToString().Length + 5, storedUser.Length - Context.Message.Author.Id.ToString().Length - 5));

                    if (userLastUseDate.AddHours(24) < DateTime.UtcNow)
                    {
                        //Add credits
                        UserBankingHandler.AddCredits(Context, dailyAmount);

                        //Write last daily use date
                        var otherLastDailyCreditUsers = userLastDailyCreditStorage.Where(p => !p.Contains(Context.Message.Author.Id.ToString()));
                        var sortedLastDailyCreditUsers = otherLastDailyCreditUsers.OrderBy(x => x).ToList();

                        TaskMethods.WriteListToFile(sortedLastDailyCreditUsers, true, userDailyLastRedemmedStorageLocation);
                        TaskMethods.WriteStringToFile($"{Context.Message.Author.Id.ToString()} >>> {DateTime.UtcNow.ToString()}", false, userDailyLastRedemmedStorageLocation);

                        //Send channel message confirmation
                        await Context.Message.Channel.SendMessageAsync("You have redeemed your daily **" + dailyAmount + " Credits!**");

                    }
                    else
                    {
                        await Context.Message.Channel.SendMessageAsync("You quacker, it has not yet been 24 hours since you last redeemed");
                    }
                }
            }
        }

        //Give credits because I am a cheater
        public static void SetCredits(SocketCommandContext context, ulong guildID, ulong userID, int setAmount)
        {
            UserBankingHandler.SetCredits(context, guildID, userID, setAmount);
        }
    }
}
