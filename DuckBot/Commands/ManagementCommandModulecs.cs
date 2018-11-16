using Discord.Commands;
using DuckBot.Finance;
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

            //Stocks
            [Command("marketOverride")]
            public async Task OverrideStockMarketAsync(bool choice)
            {
                if (choice == true)
                {
                    UserMarketStocksUpdater.overrideMarketDirection = true;
                }
                else if (choice == false)
                {
                    UserMarketStocksUpdater.overrideMarketDirection = false;
                }
            }
            [Command("marketDirection")]
            public async Task StockMarketHeadDirectionAsync(int choice)
            {
                if (choice == 0)
                {
                    UserMarketStocksUpdater.marketDirection = 0;
                }
                else if (choice == 1)
                {
                    UserMarketStocksUpdater.marketDirection = 1;
                }
            }
            [Command("marketRandNext")]
            public async Task ChangeMarketRandNextAsync(int choice)
            {
                UserMarketStocksUpdater.randNextMax = choice;
            }
        }
    }
}
