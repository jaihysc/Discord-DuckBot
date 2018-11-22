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
            var userCreditStorage = XmlManager.FromXmlFile<UserStorage>(CoreMethod.GetFileLocation(@"\UserStorage") + @"\" + Context.Message.Author.Id + ".xml");

            await Context.Message.Channel.SendMessageAsync($"**{Context.Message.Author.ToString().Substring(0, Context.Message.Author.ToString().Length - 5)}**, You have **{UserBankingHandler.CreditCurrencyFormatter(userCreditStorage.UserInfo.UserBankingStorage.Credit)} Credits**");

        }

        public static long GetUserCredits(SocketCommandContext Context)
        {
            var userCreditStorage = XmlManager.FromXmlFile<UserStorage>(CoreMethod.GetFileLocation(@"\UserStorage") + @"\" + Context.Message.Author.Id + ".xml");

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
                await Context.Message.Author.SendMessageAsync("You do not have enough money to send || **" + UserBankingHandler.CreditCurrencyFormatter(GetUserCredits(Context)) + " Credits**");
            }
            else
            {
                long taxAmount = UserCreditsTaxHandler.TaxCollector(Context, amount);

                var recipient = Context.Guild.GetUser(MentionUtils.ParseUser(targetUser));

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
                    .AddInlineField("Deductions", $"{UserBankingHandler.CreditCurrencyFormatter(taxAmount)} ({ConfigValues.taxPercentage * 100}% Tax) \n \n -------------- \n {UserBankingHandler.CreditCurrencyFormatter(amount - taxAmount)}");

                var embed = embedBuilder.Build();

                await Context.Message.Author.SendMessageAsync("", embed: embed).ConfigureAwait(false);
                await recipient.SendMessageAsync("", embed: embed).ConfigureAwait(false);
            }
        }

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

        public static void AddCredits(SocketCommandContext Context, long addAmount, bool deductTaxes = false)
        {
            //Get user credit storage
            var userCreditStorage = XmlManager.FromXmlFile<UserStorage>(CoreMethod.GetFileLocation(@"\UserStorage") + @"\" + Context.Message.Author.Id + ".xml");

            //Calculate new credits
            long userCreditsNew = 0;
            if (deductTaxes == true)
            {
                userCreditsNew = userCreditStorage.UserInfo.UserBankingStorage.Credit + addAmount - UserCreditsTaxHandler.TaxCollector(Context, userCreditStorage.UserInfo.UserBankingStorage.Credit + addAmount);
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

        }
        public static void AddCredits(SocketCommandContext Context, ulong guildID, ulong userID, long addAmount, bool deductTaxes = false)
        {
            var guild = Context.Client.GetGuild(guildID);
            var user = guild.GetUser(userID);

            //Get user credits
            var userCreditStorage = XmlManager.FromXmlFile<UserStorage>(CoreMethod.GetFileLocation(@"\UserStorage") + @"\" + user.Id + ".xml");

            //Calculate new credits
            long userCreditsNew = 0;
            if (deductTaxes == true)
            {
                userCreditsNew = userCreditStorage.UserInfo.UserBankingStorage.Credit + addAmount - UserCreditsTaxHandler.TaxCollector(Context, userCreditStorage.UserInfo.UserBankingStorage.Credit + addAmount);
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

        }
    }
}
