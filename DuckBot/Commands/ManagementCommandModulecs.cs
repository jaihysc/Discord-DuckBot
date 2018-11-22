using Discord.Commands;
using DuckBot.Finance;
using DuckBot.Finance.ServiceThreads;
using DuckBot_ClassLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DuckBot.Commands
{
    [RequireOwner]
    public class ManagementCommandModulecs : ModuleBase<SocketCommandContext>
    {
        [Group("manage")]
        public class Management : ModuleBase<SocketCommandContext>
        {
            [Command("setGame")]
            public async Task SetGameAsync([Remainder]string game)
            {
                ConfigValues.botCommandPrefix = game;
                await Context.Client.SetGameAsync($"Use {game} help");
            }

            [Command("stopService")]
            public async Task StopServicesAsync()
            {
                MainProgram._stopThreads = true;
            }
            [Command("startService")]
            public async Task StartServicesAsync()
            {
                MainProgram._stopThreads = false;
            }

            //Banking
            [Command("updateDebt")]
            public async Task UpdateUserDebtInterestAsync()
            {
                UserBankingInterestUpdater.UserDebtInterestUpdater();
            }

            [Command("taxRate")]
            public async Task ChangeTaxRateAsync(double taxRate)
            {
                ConfigValues.taxPercentage = taxRate;
            }

            [Command("maxBorrow")]
            public async Task ChangeMaxBorrowLimitAsync(long maxBorrow)
            {
                ConfigValues.maxBorrowAmount = maxBorrow;
            }

            //Duck access moderation
            [Command("op")]
            public async Task WhiteListUserAsync(ulong userId)
            {
                CoreMethod.WriteStringToFile(userId.ToString(), false, CoreMethod.GetFileLocation("UserWhitelist.txt"));
            }

            [Command("unop")]
            public async Task UnWhiteListUserAsync(ulong userId)
            {
                var filteredWhitelist = CoreMethod.ReadFromFileToList(CoreMethod.GetFileLocation("UserWhitelist.txt"));
                filteredWhitelist = filteredWhitelist.Where(u => u != userId.ToString()).ToList();
                CoreMethod.WriteListToFile(filteredWhitelist, true, CoreMethod.GetFileLocation("UserWhitelist.txt"));
            }

            [Command("block")]
            public async Task BlackListUserAsync(ulong userId)
            {
                CoreMethod.WriteStringToFile(userId.ToString(), false, CoreMethod.GetFileLocation("UserBlacklist.txt"));
            }

            [Command("unblock")]
            public async Task UnBlackListUserAsync(ulong userId)
            {
                var filteredBlacklist = CoreMethod.ReadFromFileToList(CoreMethod.GetFileLocation("UserBlacklist.txt"));
                filteredBlacklist = filteredBlacklist.Where(u => u != userId.ToString()).ToList();
                CoreMethod.WriteListToFile(filteredBlacklist, true, CoreMethod.GetFileLocation("UserBlacklist.txt"));
            }
        }
    }
}
