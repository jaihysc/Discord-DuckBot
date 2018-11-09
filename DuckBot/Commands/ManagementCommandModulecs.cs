using Discord.Commands;
using DuckBot.Finance.ServiceThreads;
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
        }
    }
}
