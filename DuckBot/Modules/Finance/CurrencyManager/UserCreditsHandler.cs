using Discord;
using Discord.Commands;
using DuckBot.Core;
using DuckBot.Models;
using DuckBot.Modules.UserActions;
using DuckBot_ClassLibrary;
using DuckBot_ClassLibrary.Modules;
using System;
using System.IO;
using System.Threading.Tasks;

namespace DuckBot.Modules.Finance.CurrencyManager
{
    public class UserCreditsHandler
    {
        /// <summary>
        /// Returns the credits the specified user has
        /// </summary>
        /// <param name="Context">This is the user, typically the sender</param>
        /// <returns></returns>
        public static long GetUserCredits(SocketCommandContext Context)
        {
            var userCreditStorage = XmlManager.FromXmlFile<UserStorage>(CoreMethod.GetFileLocation(@"\UserStorage") + @"\" + Context.Message.Author.Id + ".xml");

            return userCreditStorage.UserInfo.UserBankingStorage.Credit;
        }
        /// <summary>
        /// Returns the credits the specified user has
        /// </summary>
        /// <param name="userID">ID of the user to get</param>
        /// <returns></returns>
        public static long GetUserCredits(ulong userID)
        {
            var userCreditStorage = XmlManager.FromXmlFile<UserStorage>(CoreMethod.GetFileLocation(@"\UserStorage") + @"\" + userID + ".xml");

            return userCreditStorage.UserInfo.UserBankingStorage.Credit;
        }

        /// <summary>
        /// Transfers credits from sender to target receiver
        /// </summary>
        /// <param name="Context">Sender, typically the one who initiated the command</param>
        /// <param name="targetUser">A @mention of the receiver</param>
        /// <param name="amount">Amount to send to the receiver</param>
        /// <returns></returns>
        public static async Task TransferCredits(SocketCommandContext Context, string targetUser, long amount)
        {
            if (amount <= 0)
            {
                await Context.Message.Author.SendMessageAsync("You must send **1 or more** Credits**");
            }
            else if (GetUserCredits(Context) - amount < 0)
            {
                await Context.Message.Author.SendMessageAsync("You do not have enough money to send || **" + UserBankingHandler.CreditCurrencyFormatter(GetUserCredits(Context)) + " Credits**");
            }
            else
            {
                long taxAmount = UserCreditsTaxHandler.TaxCollector(amount);

                var recipient = Context.Guild.GetUser(MentionUtils.ParseUser(targetUser));

                //Check if recipient has a profile
                UserBankingHandler.CheckIfUserCreditProfileExists(recipient);

                //Subtract money from sender
                AddCredits(Context, -amount);

                //AddCredits credits to receiver
                AddCredits(Context, Context.Guild.Id, MentionUtils.ParseUser(targetUser), amount - taxAmount);

                //Send receipts to both parties
                var embedBuilder = new EmbedBuilder()
                    .WithTitle("Transaction Receipt")
                    .WithDescription("​")
                    .WithColor(new Color(68, 199, 40))
                    .WithFooter(footer => {
                    })
                    .WithAuthor(author => {
                        author
                            .WithName("Duck Banking Inc.")
                            .WithIconUrl("https://freeiconshop.com/wp-content/uploads/edd/bank-flat.png");
                    })
                    .AddInlineField("Sender", Context.Message.Author.ToString().Substring(0, Context.Message.Author.ToString().Length - 5))
                    .AddInlineField("Id", Context.Message.Author.Id)
                    .AddInlineField("Total Amount", $"-{UserBankingHandler.CreditCurrencyFormatter(amount)}")

                    .AddInlineField("Recipient", recipient.ToString().Substring(0, recipient.ToString().Length - 5))
                    .AddInlineField("​", recipient.Id)
                    .AddInlineField("​", UserBankingHandler.CreditCurrencyFormatter(amount))

                    .AddInlineField("​", "​")
                    .AddInlineField("​", "​")
                    .AddInlineField("Deductions", $"{UserBankingHandler.CreditCurrencyFormatter(taxAmount)} ({double.Parse(SettingsManager.RetrieveFromConfigFile("taxRate")) * 100}% Tax) \n \n -------------- \n {UserBankingHandler.CreditCurrencyFormatter(amount - taxAmount)}");

                var embed = embedBuilder.Build();

                await Context.Message.Author.SendMessageAsync("", embed: embed).ConfigureAwait(false);
                await recipient.SendMessageAsync("", embed: embed).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Sets input amount to user balance
        /// </summary>
        /// <param name="Context">This is the user, typically the sender</param>
        /// <param name="setAmount">Amount to set credits balance to</param>
        public static void SetCredits(SocketCommandContext Context, long setAmount)
        {
            var otherCreditStorageUsers = XmlManager.FromXmlFile<UserStorage>(CoreMethod.GetFileLocation(@"\UserStorage") + @"\" + Context.Message.Author.Id + ".xml");

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

            XmlManager.ToXmlFile(userRecord, CoreMethod.GetFileLocation(@"\UserStorage") + @"\" + Context.Message.Author.Id + ".xml");
        }
        /// <summary>
        /// Sets input amount to user balance
        /// </summary>
        /// <param name="userId">Target user ID</param>
        /// <param name="setAmount">Amount to set credits balance to</param>
        public static void SetCredits(ulong userId, long setAmount)
        {
            var otherCreditStorageUsers = XmlManager.FromXmlFile<UserStorage>(CoreMethod.GetFileLocation(@"\UserStorage") + @"\" + userId + ".xml");

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

            XmlManager.ToXmlFile(userRecord, CoreMethod.GetFileLocation(@"\UserStorage") + @"\" + userId + ".xml");
        }
        /// <summary>
        /// Sets input amount to user balance
        /// </summary>
        /// <param name="Context">Used to determine channel to send messages to if necessary</param>
        /// <param name="guildID">Guild ID where the target user is in</param>
        /// <param name="userID">Target user ID</param>
        /// <param name="setAmount">Amount to set credits balance to</param>
        public static void SetCredits(SocketCommandContext Context, ulong guildID, ulong userID, long setAmount)
        {
            //Get user credits to list
            var guild = Context.Client.GetGuild(guildID);
            var user = guild.GetUser(userID);

            //Get user credit storage
            var userCreditStorage = XmlManager.FromXmlFile<UserStorage>(CoreMethod.GetFileLocation(@"\UserStorage") + @"\" + user.Id + ".xml");

            //Write new user credits to file
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

            XmlManager.ToXmlFile(userRecord, CoreMethod.GetFileLocation(@"\UserStorage") + @"\" + user.Id + ".xml");
        }

        /// <summary>
        /// Adds input amount to user balance
        /// </summary>
        /// <param name="Context">This is the user, typically the sender</param>
        /// <param name="addAmount">Amount to add</param>
        /// <param name="deductTaxes">Whether or not deduct taxes from the add amount, tax rate is set in FinanceConfigValues</param>
        /// <returns></returns>
        public static bool AddCredits(SocketCommandContext Context, long addAmount, bool deductTaxes = false)
        {
            //Get user credit storage
            var userCreditStorage = XmlManager.FromXmlFile<UserStorage>(CoreMethod.GetFileLocation(@"\UserStorage") + @"\" + Context.Message.Author.Id + ".xml");

            //Check if user has sufficient credits
            if (GetUserCredits(Context) + addAmount > 0)
            {
                //Calculate new credits
                long userCreditsNew = 0;
                if (deductTaxes == true)
                {
                    userCreditsNew = userCreditStorage.UserInfo.UserBankingStorage.Credit + addAmount - UserCreditsTaxHandler.TaxCollector(userCreditStorage.UserInfo.UserBankingStorage.Credit + addAmount);
                }
                else if (deductTaxes == false)
                {
                    userCreditsNew = userCreditStorage.UserInfo.UserBankingStorage.Credit + addAmount;
                }

                //write new user credits to file
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

                XmlManager.ToXmlFile(userRecord, CoreMethod.GetFileLocation(@"\UserStorage") + @"\" + Context.Message.Author.Id + ".xml");

                return true;
            }
            else
            {
                return false;
            }
        }
        /// <summary>
        /// Adds input amount to user balance, Note: deductTaxes is not supported with this overload, use one with SocketCommandContext for that functionality
        /// </summary>
        /// <param name="userID">Target user's discord ID</param>
        /// <param name="addAmount">Amount to add</param>
        /// <returns></returns>
        public static bool AddCredits(ulong userID, long addAmount)
        {
            //Get user credits
            var userCreditStorage = XmlManager.FromXmlFile<UserStorage>(CoreMethod.GetFileLocation(@"\UserStorage") + @"\" + userID + ".xml");

            //Check if user has sufficient credits
            if (GetUserCredits(userID) + addAmount > 0)
            {
                //Calculate new credits
                long userCreditsNew = 0;
                userCreditsNew = userCreditStorage.UserInfo.UserBankingStorage.Credit + addAmount;

                //write new user credits to file
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

                XmlManager.ToXmlFile(userRecord, CoreMethod.GetFileLocation(@"\UserStorage") + @"\" + userID + ".xml");

                return true;
            }
            else
            {
                return false;
            }
        }
        /// <summary>
        /// Adds input amount to user balance
        /// </summary>
        /// <param name="Context">Used to determine channel to send messages to if necessary</param>
        /// <param name="guildID">Guild ID where the target user is in</param>
        /// <param name="userID">Target user ID</param>
        /// <param name="addAmount">Amount to add</param>
        /// <param name="deductTaxes">Whether or not deduct taxes from the add amount, tax rate is set in FinaceConfigValues</param>
        /// <returns></returns>
        public static bool AddCredits(SocketCommandContext Context, ulong guildID, ulong userID, long addAmount, bool deductTaxes = false)
        {
            var guild = Context.Client.GetGuild(guildID);
            var user = guild.GetUser(userID);

            //Get user credits
            var userCreditStorage = XmlManager.FromXmlFile<UserStorage>(CoreMethod.GetFileLocation(@"\UserStorage") + @"\" + user.Id + ".xml");

            //Check if user has sufficient credits
            if (GetUserCredits(Context) + addAmount > 0)
            {
                //Calculate new credits
                long userCreditsNew = 0;
                if (deductTaxes == true)
                {
                    userCreditsNew = userCreditStorage.UserInfo.UserBankingStorage.Credit + addAmount - UserCreditsTaxHandler.TaxCollector(userCreditStorage.UserInfo.UserBankingStorage.Credit + addAmount);
                }
                else if (deductTaxes == false)
                {
                    userCreditsNew = userCreditStorage.UserInfo.UserBankingStorage.Credit + addAmount;
                }

                //write new user credits to file
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

                XmlManager.ToXmlFile(userRecord, CoreMethod.GetFileLocation(@"\UserStorage") + @"\" + user.Id + ".xml");

                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
