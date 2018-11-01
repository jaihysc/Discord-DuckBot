using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DuckBot.Finance
{
    public class UserBankingHandler
    {
        public static string userCreditsStorageLocation = TaskMethods.GetFileLocation("UserCredits.txt");
        internal static int startingCredits = 10000;


        //Methods

        public static void CheckIfUserCreditProfileExists(SocketCommandContext Context)
        {
            var userLastDailyCreditStorage = TaskMethods.ReadFromFileToList("UserCredits.txt");

            bool userExists = false;
            foreach (var storedUser in userLastDailyCreditStorage)
            {
                bool storedUserLengthIsGreaterThanMessageAuthor = false;
                if (Context.Message.Author.ToString().Length <= storedUser.Length) storedUserLengthIsGreaterThanMessageAuthor = true;

                if (storedUserLengthIsGreaterThanMessageAuthor == true && Context.Message.Author.ToString() == storedUser.Substring(0, Context.Message.Author.ToString().Length))
                {
                    userExists = true;
                }
            }

            //Create txt user credit entry if user does not exist
            if (userExists == false)
            {
                //Create user profile
                UserBankingHandler.CreateUserCreditProfile(Context);
            }
        }

        public static void CreateUserCreditProfile(SocketCommandContext Context)
        {
            //Get user credits to list
            var userCreditStorage = TaskMethods.ReadFromFileToList("UserCredits.txt");
            var userLastDailyRedeemDateStorage = TaskMethods.ReadFromFileToList("UserCreditsDailyLastUsed.txt");

            //Write new user
            TaskMethods.WriteListToFile(userCreditStorage, true, userCreditsStorageLocation);
            TaskMethods.WriteStringToFile($"{Context.Message.Author.ToString()} >>> " + startingCredits, false, userCreditsStorageLocation);

            //Create user last daily redeem date
            TaskMethods.WriteListToFile(userLastDailyRedeemDateStorage, true, TaskMethods.GetFileLocation("UserCreditsDailyLastUsed.txt"));
            TaskMethods.WriteStringToFile($"{Context.Message.Author.ToString()} >>> {DateTime.UtcNow.AddYears(-1)}", false, TaskMethods.GetFileLocation("UserCreditsDailyLastUsed.txt"));
        }

        public static int GetUserCredits(SocketCommandContext Context)
        {
            var userCreditStorage = TaskMethods.ReadFromFileToList("UserCredits.txt");

            string selectedUser = userCreditStorage.First(x => x.Contains(Context.Message.Author.ToString()));
            return
                int.Parse(
                selectedUser.Substring(Context.Message.Author.ToString().Length + 5,
                selectedUser.Length - Context.Message.Author.ToString().Length - 5));
        }

        public static async Task DisplayUserCredits(SocketCommandContext Context)
        {
            var userCreditStorage = TaskMethods.ReadFromFileToList("UserCredits.txt");

            //Check if user exists
            bool userExists = false;
            foreach (var item in userCreditStorage)
            {
                if (item.Contains(Context.Message.Author.ToString()))
                {
                    userExists = true;
                }
            }

            if (userExists == true)
            {
                string selectedUser = userCreditStorage.First(x => x.Contains(Context.Message.Author.ToString()));
                await Context.Message.Channel.SendMessageAsync($"You have **{ selectedUser.Substring(Context.Message.Author.ToString().Length + 5, selectedUser.Length - Context.Message.Author.ToString().Length - 5)} credits**");
            }

        }

        public static async Task TransferCredits(SocketCommandContext Context, string targetUser, int amount)
        {
            if (GetUserCredits(Context) - amount < 0)
            {
                await Context.Message.Author.SendMessageAsync("You do not have enough money to send || **" + GetUserCredits(Context) + " Credits**");
            }
            else
            {
                //Subtract money from sender
                SubtractCredits(Context, amount, userCreditsStorageLocation);
                //Send receipt to sender
                await TaxCollectorAsync(
                        Context,
                        Context.Guild.Id,
                        Context.Message.Author.Id,
                        amount,
                        "You successfully sent **" + amount + " Credits** to " + targetUser);


                var recipient = Context.Guild.GetUser(MentionUtils.ParseUser(targetUser));
                //AddCredits credits to receiver
                AddCredits(
                    Context,
                    Context.Guild.Id, MentionUtils.ParseUser(targetUser),
                    //Collect taxes!
                    amount - await TaxCollectorAsync(
                        Context,
                        recipient.Guild.Id,
                        recipient.Id,
                        amount, 
                        "You received **" + amount + " Credits** from " + Context.Message.Author.Mention),
                    userCreditsStorageLocation);

                //await recipient.SendMessageAsync("You received **" + amount + " Credits** from " + Context.Message.Author.Mention);
            }
        }

        //Taxes- everyone loves em
        public static async Task<int> TaxCollectorAsync(SocketCommandContext Context, int inputCredits,string sendMessage)
        {
            //Percentage between 0-1, expressed as a fraction
            double taxPercentage = 0.13;

            double taxSubtractions = inputCredits * taxPercentage;

            if (taxSubtractions < 0)
            {
                taxSubtractions = 0;
            }

            int roundedTaxSubtractions = Convert.ToInt32(taxSubtractions);
            await Context.Message.Channel.SendMessageAsync(sendMessage + " || A total of **" + roundedTaxSubtractions + " Credits** was taken off as tax");

            return roundedTaxSubtractions;
        }
        public static async Task<int> TaxCollectorAsync(SocketCommandContext Context, ulong guildID, ulong userID, int inputCredits, string sendMessage)
        {
            var guild = Context.Client.GetGuild(guildID);
            var user = guild.GetUser(userID);

            //Percentage between 0-1, expressed as a fraction
            double taxPercentage = 0.13;

            double taxSubtractions = inputCredits * taxPercentage;

            if (taxSubtractions < 0)
            {
                taxSubtractions = 0;
            }

            int roundedTaxSubtractions = Convert.ToInt32(taxSubtractions);
            await user.SendMessageAsync(sendMessage + " || A total of **" + roundedTaxSubtractions + " Credits** was taken off as tax");

            return roundedTaxSubtractions;
        }

        //User credit functions
        public static void SetCredits(SocketCommandContext Context, int setAmount, string filePath)
        {
            //Get user credits to list
            var userCreditStorage = TaskMethods.ReadFromFileToList("UserCredits.txt");

            //Get user in txt file and add selected number of credits
            foreach (var storedUser in userCreditStorage)
            {
                //The task will cycle through every entry to find the one matching message sender
                //Check if selected user is equal to the message sender
                bool storedUserLengthIsGreaterThanMessageAuthor = false;
                if (Context.Message.Author.ToString().Length <= storedUser.Length) storedUserLengthIsGreaterThanMessageAuthor = true;

                if (storedUserLengthIsGreaterThanMessageAuthor == true && Context.Message.Author.ToString() == storedUser.Substring(0, Context.Message.Author.ToString().Length))
                {
                    var otherCreditStorageUsers = userCreditStorage.Where(p => !p.Contains(Context.Message.Author.ToString())).OrderBy(x => x).ToList();

                    TaskMethods.WriteListToFile(otherCreditStorageUsers, true, filePath);
                    TaskMethods.WriteStringToFile($"{Context.Message.Author.ToString()} >>> {setAmount}", false, filePath);
                }
            }
        }
        public static void SetCredits(SocketCommandContext Context, ulong guildID, ulong userID, int setAmount, string filePath)
        {
            //Get user credits to list
            var userCreditStorage = TaskMethods.ReadFromFileToList("UserCredits.txt");

            //Get user in txt file and add selected number of credits
            foreach (var storedUser in userCreditStorage)
            {
                var guild = Context.Client.GetGuild(guildID);
                var user = guild.GetUser(userID);

                //The task will cycle through every entry to find the one matching message sender
                //Check if selected user is equal to the message sender
                bool storedUserLengthIsGreaterThanMessageAuthor = false;
                if (user.ToString().Length <= storedUser.Length) storedUserLengthIsGreaterThanMessageAuthor = true;

                if (storedUserLengthIsGreaterThanMessageAuthor == true && user.ToString() == storedUser.Substring(0, user.ToString().Length))
                {
                    var otherCreditStorageUsers = userCreditStorage.Where(p => !p.Contains(user.ToString())).OrderBy(x => x).ToList();

                    TaskMethods.WriteListToFile(otherCreditStorageUsers, true, filePath);
                    TaskMethods.WriteStringToFile($"{user.ToString()} >>> {setAmount}", false, filePath);
                }
            }
        }

        public static void AddCredits(SocketCommandContext Context, int addAmount, string filePath)
        {
            //Get user credits to list
            var userCreditStorage = TaskMethods.ReadFromFileToList("UserCredits.txt");

            //Get user in txt file and add selected number of credits
            foreach (var storedUser in userCreditStorage)
            {
                //The task will cycle through every entry to find the one matching message sender
                //Check if selected user is equal to the message sender
                bool storedUserLengthIsGreaterThanMessageAuthor = false;
                if (Context.Message.Author.ToString().Length <= storedUser.Length) storedUserLengthIsGreaterThanMessageAuthor = true;

                if (storedUserLengthIsGreaterThanMessageAuthor == true && Context.Message.Author.ToString() == storedUser.Substring(0, Context.Message.Author.ToString().Length))
                {
                    //Extract counter behind username in txt file
                    string userCredits = storedUser.Substring(Context.Message.Author.ToString().Length + 5, storedUser.Length - Context.Message.Author.ToString().Length - 5);
                    string userCreditsNew = "";

                    //Calculate new balance
                    userCreditsNew = (int.Parse(userCredits) + addAmount).ToString();

                    var otherCreditStorageUsers = userCreditStorage.Where(p => !p.Contains(Context.Message.Author.ToString())).OrderBy(x => x).ToList();

                    TaskMethods.WriteListToFile(otherCreditStorageUsers, true, filePath);
                    TaskMethods.WriteStringToFile($"{Context.Message.Author.ToString()} >>> {userCreditsNew}", false, filePath);
                }
            }
        }
        public static void AddCredits(SocketCommandContext Context, ulong guildID, ulong userID, int addAmount, string filePath)
        {
            //Get user credits to list
            var userCreditStorage = TaskMethods.ReadFromFileToList("UserCredits.txt");

            //Get user in txt file and add selected number of credits
            foreach (var storedUser in userCreditStorage)
            {
                Console.WriteLine(storedUser);
                var guild = Context.Client.GetGuild(guildID);
                var user = guild.GetUser(userID);

                //The task will cycle through every entry to find the one matching message sender
                //Check if selected user is equal to the message sender
                bool storedUserLengthIsGreaterThanMessageAuthor = false;
                if (user.ToString().Length <= storedUser.Length) storedUserLengthIsGreaterThanMessageAuthor = true;

                if (storedUserLengthIsGreaterThanMessageAuthor == true && user.ToString() == storedUser.Substring(0, user.ToString().Length))
                {
                    //Extract counter behind username in txt file
                    string userCredits = storedUser.Substring(user.ToString().Length + 5, storedUser.Length - user.ToString().Length - 5);
                    string userCreditsNew = "";

                    //Calculate new balance
                    userCreditsNew = (int.Parse(userCredits) + addAmount).ToString();

                    var otherCreditStorageUsers = userCreditStorage.Where(p => !p.Contains(user.ToString())).OrderBy(x => x).ToList();

                    TaskMethods.WriteListToFile(otherCreditStorageUsers, true, filePath);
                    TaskMethods.WriteStringToFile($"{user.ToString()} >>> {userCreditsNew}", false, filePath);
                }
            }
        }

        public static void SubtractCredits(SocketCommandContext Context, int subtractAmount, string filePath)
        {
            //Get user credits to list
            var userCreditStorage = TaskMethods.ReadFromFileToList("UserCredits.txt");

            //Get user in txt file and add selected number of credits
            foreach (var storedUser in userCreditStorage)
            {
                //The task will cycle through every entry to find the one matching message sender
                //Check if selected user is equal to the message sender
                bool storedUserLengthIsGreaterThanMessageAuthor = false;
                if (Context.Message.Author.ToString().Length <= storedUser.Length) storedUserLengthIsGreaterThanMessageAuthor = true;

                if (storedUserLengthIsGreaterThanMessageAuthor == true && Context.Message.Author.ToString() == storedUser.Substring(0, Context.Message.Author.ToString().Length))
                {
                    //Extract counter behind username in txt file
                    string userCredits = storedUser.Substring(Context.Message.Author.ToString().Length + 5, storedUser.Length - Context.Message.Author.ToString().Length - 5);
                    string userCreditsNew = "";

                    //Calculate new balance
                    userCreditsNew = (int.Parse(userCredits) - subtractAmount).ToString();

                    var otherCreditStorageUsers = userCreditStorage.Where(p => !p.Contains(Context.Message.Author.ToString())).OrderBy(x => x).ToList();

                    TaskMethods.WriteListToFile(otherCreditStorageUsers, true, filePath);
                    TaskMethods.WriteStringToFile($"{Context.Message.Author.ToString()} >>> {userCreditsNew}", false, filePath);
                }
            }

        }

    }
}
