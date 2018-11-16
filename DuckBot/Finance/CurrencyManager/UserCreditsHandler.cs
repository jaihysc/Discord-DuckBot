using Discord;
using Discord.Commands;
using DuckBot.UserActions;
using DuckBot_ClassLibrary;
using System;
using System.IO;
using System.Threading.Tasks;

namespace DuckBot.Finance.CurrencyManager
{
    public class UserCreditsHandler
    {
        public static async Task DisplayUserCredits(SocketCommandContext Context)
        {
            var userCreditStorage = XmlManager.FromXmlFile<UserStorage>(TaskMethods.GetFileLocation(@"\UserStorage") + @"\" + Context.Message.Author.Id + ".xml");

            await Context.Message.Channel.SendMessageAsync($"You have **{userCreditStorage.UserInfo.UserBankingStorage.Credit} Credits**");

        }

        public static long GetUserCredits(SocketCommandContext Context)
        {
            var userCreditStorage = XmlManager.FromXmlFile<UserStorage>(TaskMethods.GetFileLocation(@"\UserStorage") + @"\" + Context.Message.Author.Id + ".xml");

            return userCreditStorage.UserInfo.UserBankingStorage.Credit;
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
                await UserCreditsTaxHandler.TaxCollectorAsync(
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
                    amount - await UserCreditsTaxHandler.TaxCollectorAsync(
                        Context,
                        recipient.Guild.Id,
                        recipient.Id,
                        amount,
                        "You received **" + amount + " Credits** from " + Context.Message.Author.Mention));

                //await recipient.SendMessageAsync("You received **" + amount + " Credits** from " + Context.Message.Author.Mention);
            }
        }

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
    }
}
