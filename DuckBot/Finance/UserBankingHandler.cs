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
        internal static int startingCredits = 10000;
        //Percentage between 0-1, expressed as a fraction
        internal static double taxPercentage = 0.05;

        //Methods
        public static void CheckIfUserCreditProfileExists(SocketCommandContext Context)
        {
            var userLastDailyCreditStorage = TaskMethods.ReadFromFileToList("UserCredits.txt");

            bool userExists = false;
            foreach (var storedUser in userLastDailyCreditStorage)
            {
                bool storedUserLengthIsGreaterThanMessageAuthor = false;
                if (Context.Message.Author.Id.ToString().Length <= storedUser.Length) storedUserLengthIsGreaterThanMessageAuthor = true;

                if (storedUserLengthIsGreaterThanMessageAuthor == true && Context.Message.Author.Id.ToString() == storedUser.Substring(0, Context.Message.Author.Id.ToString().Length))
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
            //Write new user credits
            var userCreditStorage = TaskMethods.ReadFromFileToList("UserCredits.txt");
            TaskMethods.WriteListToFile(userCreditStorage, true, TaskMethods.GetFileLocation("UserCredits.txt"));
            TaskMethods.WriteStringToFile($"{Context.Message.Author.Id.ToString()} >>> " + startingCredits, false, TaskMethods.GetFileLocation("UserCredits.txt"));

            //Write new user debt
            var userCreditDebtStorage = TaskMethods.ReadFromFileToList("UserCreditsDebt.txt");
            TaskMethods.WriteListToFile(userCreditDebtStorage, true, TaskMethods.GetFileLocation("UserCredits.txt"));
            TaskMethods.WriteStringToFile($"{Context.Message.Author.Id.ToString()} >>> 0", false, TaskMethods.GetFileLocation("UserCreditsDebt.txt"));

            //Create user last daily redeem date
            var userLastDailyRedeemDateStorage = TaskMethods.ReadFromFileToList("UserCreditsDailyLastUsed.txt");
            TaskMethods.WriteListToFile(userLastDailyRedeemDateStorage, true, TaskMethods.GetFileLocation("UserCreditsDailyLastUsed.txt"));
            TaskMethods.WriteStringToFile($"{Context.Message.Author.Id.ToString()} >>> {DateTime.UtcNow.AddYears(-1)}", false, TaskMethods.GetFileLocation("UserCreditsDailyLastUsed.txt"));
        }

        public static int GetUserCredits(SocketCommandContext Context)
        {
            var userCreditStorage = TaskMethods.ReadFromFileToList("UserCredits.txt");

            string selectedUser = userCreditStorage.First(x => x.Contains(Context.Message.Author.Id.ToString()));
            return
                int.Parse(
                selectedUser.Substring(Context.Message.Author.Id.ToString().Length + 5,
                selectedUser.Length - Context.Message.Author.Id.ToString().Length - 5));
        }

        //Banking
        public static async Task DisplayUserCredits(SocketCommandContext Context)
        {
            var userCreditStorage = TaskMethods.ReadFromFileToList("UserCredits.txt");

            string selectedUser = userCreditStorage.First(x => x.Contains(Context.Message.Author.Id.ToString()));
            await Context.Message.Channel.SendMessageAsync($"You have **{ selectedUser.Substring(Context.Message.Author.Id.ToString().Length + 5, selectedUser.Length - Context.Message.Author.Id.ToString().Length - 5)} Credits**");

        }

        public static async Task TransferCredits(SocketCommandContext Context, string targetUser, int amount)
        {
            if (amount <= 0)
            {
                await Context.Message.Author.SendMessageAsync("You must send **1 or more** Credits**");
            }
            else if (GetUserCredits(Context) - amount < 0)
            {
                await Context.Message.Author.SendMessageAsync("You do not have enough money to send || **" + GetUserCredits(Context) + " Credits**");
            }
            else
            {
                //Subtract money from sender
                SubtractCredits(Context, amount);
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
                        "You received **" + amount + " Credits** from " + Context.Message.Author.Mention));

                //await recipient.SendMessageAsync("You received **" + amount + " Credits** from " + Context.Message.Author.Mention);
            }
        }

        //Taxes- everyone loves em
        public static async Task<int> TaxCollectorAsync(SocketCommandContext Context, int inputCredits,string sendMessage)
        {

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
        public static void SetCredits(SocketCommandContext Context, int setAmount)
        {
            //Get user credits to list
            var userCreditStorage = TaskMethods.ReadFromFileToList("UserCredits.txt");

            //Get user in txt file and add selected number of credits
            foreach (var storedUser in userCreditStorage)
            {
                //The task will cycle through every entry to find the one matching message sender
                //Check if selected user is equal to the message sender
                bool storedUserLengthIsGreaterThanMessageAuthor = false;
                if (Context.Message.Author.Id.ToString().Length <= storedUser.Length) storedUserLengthIsGreaterThanMessageAuthor = true;

                if (storedUserLengthIsGreaterThanMessageAuthor == true && Context.Message.Author.Id.ToString() == storedUser.Substring(0, Context.Message.Author.Id.ToString().Length))
                {
                    var otherCreditStorageUsers = userCreditStorage.Where(p => !p.Contains(Context.Message.Author.Id.ToString())).OrderBy(x => x).ToList();

                    TaskMethods.WriteListToFile(otherCreditStorageUsers, true, TaskMethods.GetFileLocation("UserCredits.txt"));
                    TaskMethods.WriteStringToFile($"{Context.Message.Author.Id.ToString()} >>> {setAmount}", false, TaskMethods.GetFileLocation("UserCredits.txt"));
                }
            }
        }
        public static void SetCredits(SocketCommandContext Context, ulong guildID, ulong userID, int setAmount)
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
                if (user.Id.ToString().Length <= storedUser.Length) storedUserLengthIsGreaterThanMessageAuthor = true;

                if (storedUserLengthIsGreaterThanMessageAuthor == true && user.Id.ToString() == storedUser.Substring(0, user.Id.ToString().Length))
                {
                    var otherCreditStorageUsers = userCreditStorage.Where(p => !p.Contains(user.Id.ToString())).OrderBy(x => x).ToList();

                    TaskMethods.WriteListToFile(otherCreditStorageUsers, true, TaskMethods.GetFileLocation("UserCredits.txt"));
                    TaskMethods.WriteStringToFile($"{user.Id.ToString()} >>> {setAmount}", false, TaskMethods.GetFileLocation("UserCredits.txt"));
                }
            }
        }

        public static void AddCredits(SocketCommandContext Context, int addAmount)
        {
            //Get user credits to list
            var userCreditStorage = TaskMethods.ReadFromFileToList("UserCredits.txt");

            //Get user in txt file and add selected number of credits
            foreach (var storedUser in userCreditStorage)
            {
                //The task will cycle through every entry to find the one matching message sender
                //Check if selected user is equal to the message sender
                bool storedUserLengthIsGreaterThanMessageAuthor = false;
                if (Context.Message.Author.Id.ToString().Length <= storedUser.Length) storedUserLengthIsGreaterThanMessageAuthor = true;

                if (storedUserLengthIsGreaterThanMessageAuthor == true && Context.Message.Author.Id.ToString() == storedUser.Substring(0, Context.Message.Author.Id.ToString().Length))
                {
                    //Extract counter behind username in txt file
                    string userCredits = storedUser.Substring(Context.Message.Author.Id.ToString().Length + 5, storedUser.Length - Context.Message.Author.Id.ToString().Length - 5);
                    string userCreditsNew = "";

                    //Calculate new balance
                    userCreditsNew = (int.Parse(userCredits) + addAmount).ToString();

                    var otherCreditStorageUsers = userCreditStorage.Where(p => !p.Contains(Context.Message.Author.Id.ToString())).OrderBy(x => x).ToList();

                    TaskMethods.WriteListToFile(otherCreditStorageUsers, true, TaskMethods.GetFileLocation("UserCredits.txt"));
                    TaskMethods.WriteStringToFile($"{Context.Message.Author.Id.ToString()} >>> {userCreditsNew}", false, TaskMethods.GetFileLocation("UserCredits.txt"));
                }
            }
        }
        public static void AddCredits(SocketCommandContext Context, ulong guildID, ulong userID, int addAmount)
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
                if (user.Id.ToString().Length <= storedUser.Length) storedUserLengthIsGreaterThanMessageAuthor = true;

                if (storedUserLengthIsGreaterThanMessageAuthor == true && user.Id.ToString() == storedUser.Substring(0, user.Id.ToString().Length))
                {
                    //Extract counter behind username in txt file
                    string userCredits = storedUser.Substring(user.Id.ToString().Length + 5, storedUser.Length - user.Id.ToString().Length - 5);
                    string userCreditsNew = "";

                    //Calculate new balance
                    userCreditsNew = (int.Parse(userCredits) + addAmount).ToString();

                    var otherCreditStorageUsers = userCreditStorage.Where(p => !p.Contains(user.Id.ToString())).OrderBy(x => x).ToList();

                    TaskMethods.WriteListToFile(otherCreditStorageUsers, true, TaskMethods.GetFileLocation("UserCredits.txt"));
                    TaskMethods.WriteStringToFile($"{user.Id.ToString()} >>> {userCreditsNew}", false, TaskMethods.GetFileLocation("UserCredits.txt"));
                }
            }
        }

        public static void SubtractCredits(SocketCommandContext Context, int subtractAmount)
        {
            //Get user credits to list
            var userCreditStorage = TaskMethods.ReadFromFileToList("UserCredits.txt");

            //Get user in txt file and add selected number of credits
            foreach (var storedUser in userCreditStorage)
            {
                //The task will cycle through every entry to find the one matching message sender
                //Check if selected user is equal to the message sender
                bool storedUserLengthIsGreaterThanMessageAuthor = false;
                if (Context.Message.Author.Id.ToString().Length <= storedUser.Length) storedUserLengthIsGreaterThanMessageAuthor = true;

                if (storedUserLengthIsGreaterThanMessageAuthor == true && Context.Message.Author.Id.ToString() == storedUser.Substring(0, Context.Message.Author.Id.ToString().Length))
                {
                    //Extract counter behind username in txt file
                    string userCredits = storedUser.Substring(Context.Message.Author.Id.ToString().Length + 5, storedUser.Length - Context.Message.Author.Id.ToString().Length - 5);
                    string userCreditsNew = "";

                    //Calculate new balance
                    userCreditsNew = (int.Parse(userCredits) - subtractAmount).ToString();

                    var otherCreditStorageUsers = userCreditStorage.Where(p => !p.Contains(Context.Message.Author.Id.ToString())).OrderBy(x => x).ToList();

                    TaskMethods.WriteListToFile(otherCreditStorageUsers, true, TaskMethods.GetFileLocation("UserCredits.txt"));
                    TaskMethods.WriteStringToFile($"{Context.Message.Author.Id.ToString()} >>> {userCreditsNew}", false, TaskMethods.GetFileLocation("UserCredits.txt"));
                }
            }

        }
        public static void SubtractCredits(SocketCommandContext Context, ulong guildID, ulong userID, int subtractAmount)
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
                if (user.Id.ToString().Length <= storedUser.Length) storedUserLengthIsGreaterThanMessageAuthor = true;

                if (storedUserLengthIsGreaterThanMessageAuthor == true && user.Id.ToString() == storedUser.Substring(0, user.Id.ToString().Length))
                {
                    //Extract counter behind username in txt file
                    string userCredits = storedUser.Substring(user.Id.ToString().Length + 5, storedUser.Length - user.Id.ToString().Length - 5);
                    string userCreditsNew = "";

                    //Calculate new balance
                    userCreditsNew = (int.Parse(userCredits) + subtractAmount).ToString();

                    var otherCreditStorageUsers = userCreditStorage.Where(p => !p.Contains(user.Id.ToString())).OrderBy(x => x).ToList();

                    TaskMethods.WriteListToFile(otherCreditStorageUsers, true, TaskMethods.GetFileLocation("UserCredits.txt"));
                    TaskMethods.WriteStringToFile($"{user.Id.ToString()} >>> {userCreditsNew}", false, TaskMethods.GetFileLocation("UserCredits.txt"));
                }
            }
        }

        //Money borrowing & debt
        public static async Task DisplayUserCreditsDebt(SocketCommandContext Context)
        {
            var userCreditStorage = TaskMethods.ReadFromFileToList("UserCreditsDebt.txt");

            string selectedUser = userCreditStorage.First(x => x.Contains(Context.Message.Author.Id.ToString()));
            await Context.Message.Channel.SendMessageAsync($"You owe **{ selectedUser.Substring(Context.Message.Author.Id.ToString().Length + 5, selectedUser.Length - Context.Message.Author.Id.ToString().Length - 5)} Credits**");

        }

        public static int GetUserCreditsDebt(SocketCommandContext Context)
        {
            var userCreditStorage = TaskMethods.ReadFromFileToList("UserCreditsDebt.txt");

            string selectedUser = userCreditStorage.First(x => x.Contains(Context.Message.Author.Id.ToString()));
            return int.Parse(selectedUser.Substring(Context.Message.Author.Id.ToString().Length + 5, selectedUser.Length - Context.Message.Author.Id.ToString().Length - 5));
        }

        public static async Task BorrowCredits(SocketCommandContext Context, int borrowAmount)
        {
            int maxBorrowAmount = 100000;

            if (GetUserCreditsDebt(Context) + borrowAmount > maxBorrowAmount)
            {
                await Context.Message.Channel.SendMessageAsync($"You have exceeded your credit limit of **{maxBorrowAmount} Credits**");
            }
            else if (borrowAmount <= 0)
            {
                await Context.Message.Channel.SendMessageAsync($"You have to borrow **1 or more** Credits");
            }
            else
            {
                //Add to debt counter
                AddDebt(Context, borrowAmount);
                //Add credits to user
                AddCredits(Context, borrowAmount);

                //Send receipt
                await Context.Message.Channel.SendMessageAsync($"You borrowed **{borrowAmount} Credits**");
            }
        }

        public static async Task ReturnCredits(SocketCommandContext Context, int returnAmount)
        {
            if (returnAmount > GetUserCreditsDebt(Context))
            {
                await Context.Message.Channel.SendMessageAsync($"You do not owe **{returnAmount} Credits** || **{GetUserCreditsDebt(Context)} Credits**");
            }
            else if (returnAmount <= 0)
            {
                await Context.Message.Channel.SendMessageAsync($"You have to pay back **1 or more** Credits");
            }
            else if (returnAmount > GetUserCredits(Context))
            {
                await Context.Message.Channel.SendMessageAsync($"You do not have enough credits to pay back || **{GetUserCredits(Context)}** Credits");
            }
            else
            {
                //Subtract from debt counter
                SubtractDebt(Context, returnAmount);
                //Subtract credits to user
                SubtractCredits(Context, returnAmount);

                //Send receipt
                await Context.Message.Channel.SendMessageAsync($"You paid back **{returnAmount} Credits**");
            }
        }


        public static void AddDebt(SocketCommandContext Context, int addAmount)
        {
            //Get user debt to list
            var userCreditDebtStorage = TaskMethods.ReadFromFileToList("UserCreditsDebt.txt");

            //Get user in txt file and add selected number of credits
            foreach (var storedUser in userCreditDebtStorage)
            {
                //The task will cycle through every entry to find the one matching message sender
                //Check if selected user is equal to the message sender
                bool storedUserLengthIsGreaterThanMessageAuthor = false;
                if (Context.Message.Author.Id.ToString().Length <= storedUser.Length) storedUserLengthIsGreaterThanMessageAuthor = true;

                if (storedUserLengthIsGreaterThanMessageAuthor == true && Context.Message.Author.Id.ToString() == storedUser.Substring(0, Context.Message.Author.Id.ToString().Length))
                {
                    //Extract counter behind username in txt file
                    string userCredits = storedUser.Substring(Context.Message.Author.Id.ToString().Length + 5, storedUser.Length - Context.Message.Author.Id.ToString().Length - 5);
                    string userCreditsDebtNew = "";

                    //Calculate new debt balance
                    userCreditsDebtNew = (int.Parse(userCredits) + addAmount).ToString();

                    var otherCreditStorageUsers = userCreditDebtStorage.Where(p => !p.Contains(Context.Message.Author.Id.ToString())).OrderBy(x => x).ToList();

                    TaskMethods.WriteListToFile(otherCreditStorageUsers, true, TaskMethods.GetFileLocation("UserCreditsDebt.txt"));
                    TaskMethods.WriteStringToFile($"{Context.Message.Author.Id.ToString()} >>> {userCreditsDebtNew}", false, TaskMethods.GetFileLocation("UserCreditsDebt.txt"));
                }
            }
        }

        public static void SubtractDebt(SocketCommandContext Context, int addAmount)
        {
            //Get user debt to list
            var userCreditDebtStorage = TaskMethods.ReadFromFileToList("UserCreditsDebt.txt");

            //Get user in txt file and add selected number of credits
            foreach (var storedUser in userCreditDebtStorage)
            {
                //The task will cycle through every entry to find the one matching message sender
                //Check if selected user is equal to the message sender
                bool storedUserLengthIsGreaterThanMessageAuthor = false;
                if (Context.Message.Author.Id.ToString().Length <= storedUser.Length) storedUserLengthIsGreaterThanMessageAuthor = true;

                if (storedUserLengthIsGreaterThanMessageAuthor == true && Context.Message.Author.Id.ToString() == storedUser.Substring(0, Context.Message.Author.Id.ToString().Length))
                {
                    //Extract counter behind username in txt file
                    string userCredits = storedUser.Substring(Context.Message.Author.Id.ToString().Length + 5, storedUser.Length - Context.Message.Author.Id.ToString().Length - 5);
                    string userCreditsDebtNew = "";

                    //Calculate new debt balance
                    userCreditsDebtNew = (int.Parse(userCredits) - addAmount).ToString();

                    var otherCreditStorageUsers = userCreditDebtStorage.Where(p => !p.Contains(Context.Message.Author.Id.ToString())).OrderBy(x => x).ToList();

                    TaskMethods.WriteListToFile(otherCreditStorageUsers, true, TaskMethods.GetFileLocation("UserCreditsDebt.txt"));
                    TaskMethods.WriteStringToFile($"{Context.Message.Author.Id.ToString()} >>> {userCreditsDebtNew}", false, TaskMethods.GetFileLocation("UserCreditsDebt.txt"));
                }
            }
        }
    }
}
