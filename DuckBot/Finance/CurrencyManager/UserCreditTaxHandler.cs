﻿using Discord;
using Discord.Commands;
using DuckBot.UserActions;
using DuckBot_ClassLibrary;
using System;
using System.IO;
using System.Threading.Tasks;

namespace DuckBot.Finance.CurrencyManager
{
    public class UserCreditsTaxHandler
    {
        public static long TaxCollector(SocketCommandContext Context, long inputCredits)
        {

            double taxSubtractions = inputCredits * ConfigValues.taxPercentage;

            if (taxSubtractions < 0)
            {
                taxSubtractions = 0;
            }

            long roundedTaxSubtractions = Convert.ToInt64(taxSubtractions);

            return roundedTaxSubtractions;
        }
        public static async Task<long> TaxCollector(SocketCommandContext Context, long inputCredits, string sendMessage)
        {

            double taxSubtractions = inputCredits * ConfigValues.taxPercentage;

            if (taxSubtractions < 0)
            {
                taxSubtractions = 0;
            }

            long roundedTaxSubtractions = Convert.ToInt64(taxSubtractions);
            await Context.Message.Channel.SendMessageAsync(sendMessage + " || A total of **" + UserBankingHandler.CreditCurrencyFormatter(roundedTaxSubtractions) + " Credits** was taken off as tax");

            return roundedTaxSubtractions;
        }
        public static long TaxCollector(SocketCommandContext Context, ulong guildID, ulong userID, long inputCredits)
        {
            var guild = Context.Client.GetGuild(guildID);
            var user = guild.GetUser(userID);

            double taxSubtractions = inputCredits * ConfigValues.taxPercentage; ;

            if (taxSubtractions < 0)
            {
                taxSubtractions = 0;
            }

            long roundedTaxSubtractions = Convert.ToInt64(taxSubtractions);

            return roundedTaxSubtractions;
        }
        public static async Task<long> TaxCollector(SocketCommandContext Context, ulong guildID, ulong userID, long inputCredits, string sendMessage)
        {
            var guild = Context.Client.GetGuild(guildID);
            var user = guild.GetUser(userID);

            double taxSubtractions = inputCredits * ConfigValues.taxPercentage; ;

            if (taxSubtractions < 0)
            {
                taxSubtractions = 0;
            }

            long roundedTaxSubtractions = Convert.ToInt64(taxSubtractions);
            await user.SendMessageAsync(sendMessage + " || A total of **" + UserBankingHandler.CreditCurrencyFormatter(roundedTaxSubtractions) + " Credits** was taken off as tax");

            return roundedTaxSubtractions;
        }
    }
}
