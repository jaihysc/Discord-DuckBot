﻿using Discord;
using Discord.Commands;
using DuckBot.UserActions;
using DuckBot_ClassLibrary;
using System;
using System.IO;
using System.Threading.Tasks;

namespace DuckBot.Finance.CurrencyManager
{
    public class UserDebtHandler
    {
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
            if (GetUserCreditsDebt(Context) + borrowAmount > ConfigValues.maxBorrowAmount)
            {
                await Context.Message.Channel.SendMessageAsync($"You have exceeded your credit limit of **{ConfigValues.maxBorrowAmount} Credits**");
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
                UserCreditsHandler.AddCredits(Context, borrowAmount);

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
            else if (returnAmount > UserCreditsHandler.GetUserCredits(Context))
            {
                await Context.Message.Channel.SendMessageAsync($"You do not have enough credits to pay back || **{UserCreditsHandler.GetUserCredits(Context)}** Credits");
            }
            else
            {
                //Subtract from debt counter
                AddDebt(Context, -returnAmount);
                //Subtract credits to user
                UserCreditsHandler.AddCredits(Context, -returnAmount);

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