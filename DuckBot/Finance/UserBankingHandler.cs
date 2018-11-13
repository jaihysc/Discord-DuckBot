using Discord;
using Discord.Commands;
using Discord.WebSocket;
using DuckBot.UserActions;
using DuckBot_ClassLibrary;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DuckBot.Finance
{
    public class UserBankingHandler
    {
        internal static long startingCredits = 10000;
        //Percentage between 0-1, expressed as a fraction
        public static double taxPercentage = 0.10;

        public static long maxBorrowAmount = 5000000;


        //Methods
        public static void CheckIfUserCreditProfileExists(SocketCommandContext Context)
        {
            //Create txt user credit entry if user does not exist
            if (!File.Exists(TaskMethods.GetFileLocation(@"\UserStorage") + @"\" + Context.Message.Author.Id + ".xml"))
            {
                //Create user profile
                UserXmlDataStorage.CreateNewUserXmlEntry(Context);
            }
        }
        public static long GetUserCredits(SocketCommandContext Context)
        {
            var userCreditStorage = XmlManager.FromXmlFile<UserStorage>(TaskMethods.GetFileLocation(@"\UserStorage") + @"\" + Context.Message.Author.Id + ".xml");

            return userCreditStorage.UserInfo.UserBankingStorage.Credit;
        }

        //Banking
        public static async Task DisplayUserCredits(SocketCommandContext Context)
        {
            var userCreditStorage = XmlManager.FromXmlFile<UserStorage>(TaskMethods.GetFileLocation(@"\UserStorage") + @"\" + Context.Message.Author.Id + ".xml");

            await Context.Message.Channel.SendMessageAsync($"You have **{userCreditStorage.UserInfo.UserBankingStorage.Credit} Credits**");

        }

        public static async Task TransferCredits(SocketCommandContext Context, string targetUser, long amount)
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
                AddCredits(Context, -amount);
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
        public static async Task<long> TaxCollectorAsync(SocketCommandContext Context, long inputCredits,string sendMessage)
        {

            double taxSubtractions = inputCredits * taxPercentage;

            if (taxSubtractions < 0)
            {
                taxSubtractions = 0;
            }

            long roundedTaxSubtractions = Convert.ToInt64(taxSubtractions);
            await Context.Message.Channel.SendMessageAsync(sendMessage + " || A total of **" + roundedTaxSubtractions + " Credits** was taken off as tax");

            return roundedTaxSubtractions;
        }
        public static async Task<long> TaxCollectorAsync(SocketCommandContext Context, ulong guildID, ulong userID, long inputCredits, string sendMessage)
        {
            var guild = Context.Client.GetGuild(guildID);
            var user = guild.GetUser(userID);

            double taxSubtractions = inputCredits * taxPercentage; ;

            if (taxSubtractions < 0)
            {
                taxSubtractions = 0;
            }

            long roundedTaxSubtractions = Convert.ToInt64(taxSubtractions);
            await user.SendMessageAsync(sendMessage + " || A total of **" + roundedTaxSubtractions + " Credits** was taken off as tax");

            return roundedTaxSubtractions;
        }

        //User credit functions
        public static void SetCredits(SocketCommandContext Context, long setAmount)
        {
            var otherCreditStorageUsers = XmlManager.FromXmlFile<UserStorage>(TaskMethods.GetFileLocation(@"\UserStorage") + @"\" + Context.Message.Author.Id + ".xml");

            var userRecord = new UserStorage
            {
                UserId = otherCreditStorageUsers.UserId,
                UserInfo = new UserInfo
                {
                    UserDailyLastUseStorage = new UserDailyLastUseStorage { DateTime = otherCreditStorageUsers.UserInfo.UserDailyLastUseStorage.DateTime },
                    UserBankingStorage = new UserBankingStorage { Credit = setAmount, CreditDebt = otherCreditStorageUsers.UserInfo.UserBankingStorage.CreditDebt },
                    UserProhibitedWordsStorage = new UserProhibitedWordsStorage { SwearCount = otherCreditStorageUsers.UserInfo.UserProhibitedWordsStorage.SwearCount }
                }
            };

            XmlManager.ToXmlFile(userRecord, TaskMethods.GetFileLocation(@"\UserStorage") + @"\" + Context.Message.Author.Id + ".xml");
        }
        public static void SetCredits(SocketCommandContext Context, ulong guildID, ulong userID, long setAmount)
        {
            //Get user credits to list
            var guild = Context.Client.GetGuild(guildID);
            var user = guild.GetUser(userID);

            var userCreditStorage = XmlManager.FromXmlFile<UserStorage>(TaskMethods.GetFileLocation(@"\UserStorage") + @"\" + user.Id + ".xml");

            var userRecord = new UserStorage
            {
                UserId = userCreditStorage.UserId,
                UserInfo = new UserInfo
                {
                    UserDailyLastUseStorage = new UserDailyLastUseStorage { DateTime = userCreditStorage.UserInfo.UserDailyLastUseStorage.DateTime },
                    UserBankingStorage = new UserBankingStorage { Credit = setAmount, CreditDebt = userCreditStorage.UserInfo.UserBankingStorage.CreditDebt },
                    UserProhibitedWordsStorage = new UserProhibitedWordsStorage { SwearCount = userCreditStorage.UserInfo.UserProhibitedWordsStorage.SwearCount }
                }
            };

            XmlManager.ToXmlFile(userRecord, TaskMethods.GetFileLocation(@"\UserStorage") + @"\" + user.Id + ".xml");
        }

        public static void AddCredits(SocketCommandContext Context, long addAmount)
        {
            //Get user credits to list
            var userCreditStorage = XmlManager.FromXmlFile<UserStorage>(TaskMethods.GetFileLocation(@"\UserStorage") + @"\" + Context.Message.Author.Id + ".xml");


            //Calculate new balance
            long userCreditsNew = userCreditStorage.UserInfo.UserBankingStorage.Credit + addAmount;

            var userRecord = new UserStorage
            {
                UserId = userCreditStorage.UserId,
                UserInfo = new UserInfo
                {
                    UserDailyLastUseStorage = new UserDailyLastUseStorage { DateTime = userCreditStorage.UserInfo.UserDailyLastUseStorage.DateTime },
                    UserBankingStorage = new UserBankingStorage { Credit = userCreditsNew, CreditDebt = userCreditStorage.UserInfo.UserBankingStorage.CreditDebt },
                    UserProhibitedWordsStorage = new UserProhibitedWordsStorage { SwearCount = userCreditStorage.UserInfo.UserProhibitedWordsStorage.SwearCount }
                }
            };

            XmlManager.ToXmlFile(userRecord, TaskMethods.GetFileLocation(@"\UserStorage") + @"\" + Context.Message.Author.Id + ".xml");

        }
        public static void AddCredits(SocketCommandContext Context, ulong guildID, ulong userID, long addAmount)
        {
            var guild = Context.Client.GetGuild(guildID);
            var user = guild.GetUser(userID);

            //Get user credits to list
            var userCreditStorage = XmlManager.FromXmlFile<UserStorage>(TaskMethods.GetFileLocation(@"\UserStorage") + @"\" + user.Id + ".xml");

            //Calculate new balance
            long userCreditsNew = userCreditStorage.UserInfo.UserBankingStorage.Credit + addAmount;

            var userRecord = new UserStorage
            {
                UserId = userCreditStorage.UserId,
                UserInfo = new UserInfo
                {
                    UserDailyLastUseStorage = new UserDailyLastUseStorage { DateTime = userCreditStorage.UserInfo.UserDailyLastUseStorage.DateTime },
                    UserBankingStorage = new UserBankingStorage { Credit = userCreditsNew, CreditDebt = userCreditStorage.UserInfo.UserBankingStorage.CreditDebt },
                    UserProhibitedWordsStorage = new UserProhibitedWordsStorage { SwearCount = userCreditStorage.UserInfo.UserProhibitedWordsStorage.SwearCount }
                }
            };

            XmlManager.ToXmlFile(userRecord, TaskMethods.GetFileLocation(@"\UserStorage") + @"\" + user.Id + ".xml");

        }

        //Money borrowing & debt
        public static async Task DisplayUserCreditsDebt(SocketCommandContext Context)
        {
            var userCreditStorage = XmlManager.FromXmlFile<UserStorage>(TaskMethods.GetFileLocation(@"\UserStorage") + @"\" + Context.Message.Author.Id + ".xml");

            await Context.Message.Channel.SendMessageAsync($"You owe **{userCreditStorage.UserInfo.UserBankingStorage.CreditDebt} Credits**");

        }

        public static long GetUserCreditsDebt(SocketCommandContext Context)
        {
            var userCreditStorage = XmlManager.FromXmlFile<UserStorage>(TaskMethods.GetFileLocation(@"\UserStorage") + @"\" + Context.Message.Author.Id + ".xml");

            return userCreditStorage.UserInfo.UserBankingStorage.CreditDebt;
        }

        public static async Task BorrowCredits(SocketCommandContext Context, long borrowAmount)
        {
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

        public static async Task ReturnCredits(SocketCommandContext Context, long returnAmount)
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
                AddDebt(Context, -returnAmount);
                //Subtract credits to user
                AddCredits(Context, -returnAmount);

                //Send receipt
                await Context.Message.Channel.SendMessageAsync($"You paid back **{returnAmount} Credits**");
            }
        }


        public static void AddDebt(SocketCommandContext Context, long addAmount)
        {
            //Get user debt to list
            var userCreditDebtStorage = XmlManager.FromXmlFile<UserStorage>(TaskMethods.GetFileLocation(@"\UserStorage") + @"\" + Context.Message.Author.Id + ".xml");

            //Calculate new debt balance
            long userCreditsDebtNew = userCreditDebtStorage.UserInfo.UserBankingStorage.CreditDebt + addAmount;

            var userRecord = new UserStorage
            {
                UserId = userCreditDebtStorage.UserId,
                UserInfo = new UserInfo
                {
                    UserDailyLastUseStorage = new UserDailyLastUseStorage { DateTime = userCreditDebtStorage.UserInfo.UserDailyLastUseStorage.DateTime },
                    UserBankingStorage = new UserBankingStorage { Credit = userCreditDebtStorage.UserInfo.UserBankingStorage.Credit, CreditDebt = userCreditsDebtNew },
                    UserProhibitedWordsStorage = new UserProhibitedWordsStorage { SwearCount = userCreditDebtStorage.UserInfo.UserProhibitedWordsStorage.SwearCount }
                }
            };

            XmlManager.ToXmlFile(userRecord, TaskMethods.GetFileLocation(@"\UserStorage") + @"\" + Context.Message.Author.Id + ".xml");
        }
    }
}
