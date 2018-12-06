using Discord;
using Discord.Commands;
using DuckBot.Core;
using DuckBot.Modules.Finance;
using DuckBot.Modules.Finance.CurrencyManager;
using DuckBot.Modules.Finance.ServiceThreads;
using DuckBot_ClassLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DuckBot.Modules.Commands
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
                MainProgram.botCommandPrefix = game;
                await Context.Client.SetGameAsync($"Use {game} help");
            }

            [Command("logAllMessages")]
            public async Task LogAllMessagesAsync(ulong guildID, ulong retrieveTargetChannelID, string path)
            {
                if (Context.Message.Author.Id == 285266023475838976)
                {
                    var guild = Context.Client.GetGuild(guildID);
                    var chnl = guild.GetTextChannel(retrieveTargetChannelID);
                    var messages = await chnl.GetMessagesAsync(9999999).Flatten();
                    var logMessage = messages.Reverse();
                    foreach (var item in logMessage)
                    {
                        try
                        {
                            using (System.IO.StreamWriter file = new System.IO.StreamWriter(path, true))
                            {
                                file.WriteLine(item);
                            }
                        }
                        catch (Exception)
                        {
                            await Context.User.SendMessageAsync("Unable to log message to specified file path");
                        }
                    }
                }
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

            [Command("setCredits")]
            public async Task SetCreditsAsync(ulong userID, long setAmount)
            {
                UserCreditsHandler.SetCredits(userID, setAmount);
            }



            //Configuable commands
            [Command("taxRate")]
            public async Task ChangeTaxRateAsync(double rate)
            {
                SettingsManager.WriteToConfigFile("taxRate", rate.ToString());
            }
            [Command("interestRate")]
            public async Task ChangeInterestRateAsync(double rate)
            {
                SettingsManager.WriteToConfigFile("interestRate", rate.ToString());
            }

            [Command("maxBorrow")]
            public async Task ChangeMaxBorrowLimitAsync(long amount)
            {
                SettingsManager.WriteToConfigFile("maxBorrow", amount.ToString());
            }
            [Command("dailyAmount")]
            public async Task ChangeDailyAmountAsync(long amount)
            {
                SettingsManager.WriteToConfigFile("dailyAmount", amount.ToString());
            }
            [Command("startAmount")]
            public async Task ChangeStartAmountAsync(long amount)
            {
                SettingsManager.WriteToConfigFile("startAmount", amount.ToString());
            }



            //Duck access moderation
            [Command("op")]
            public async Task WhiteListUserAsync(ulong userId)
            {
                //If user does not exist in op list, add
                if (!CoreMethod.ReadFromFileToList(CoreMethod.GetFileLocation("UserWhitelist.txt")).Contains(userId.ToString()))
                {
                    CoreMethod.WriteStringToFile(userId.ToString(), false, CoreMethod.GetFileLocation("UserWhitelist.txt"));
                }

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
                //If user does not exist in block list, block
                if (!CoreMethod.ReadFromFileToList(CoreMethod.GetFileLocation("UserBlacklist.txt")).Contains(userId.ToString()))
                {
                    CoreMethod.WriteStringToFile(userId.ToString(), false, CoreMethod.GetFileLocation("UserBlacklist.txt"));
                }
                
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
