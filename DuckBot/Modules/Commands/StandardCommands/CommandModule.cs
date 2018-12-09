using Discord.Addons.Interactive;
using Discord.Commands;
using DuckBot.Modules.Commands.Preconditions;
using DuckBot.Modules.CsgoCaseUnboxing;
using DuckBot.Modules.Finance;
using DuckBot.Modules.Finance.CurrencyManager;
using DuckBot.Modules.UserActions;
using DuckBot.Modules.UserFinance;
using DuckBot_ClassLibrary;
using System;
using System.IO;
using System.Threading.Tasks;

namespace DuckBot.Modules.Commands.StandardCommands
{
    [Ratelimit(1, 10, Measure.Seconds)]
    [BlacklistedUsersPrecondition]
    [UserStorageCheckerPrecondition]
    public class CommandModule : InteractiveBase
    {
        [Command("help")]
        public async Task HelpAsync([Remainder]string inputCommand = null)
        {
            if (!string.IsNullOrEmpty(inputCommand))
            {
                await UserHelpHandler.DisplayCommandHelpMenu(Context, inputCommand);
            }
            else
            {
                await UserHelpHandler.DisplayHelpMenu(Context);
            }
        }
        [Command("@help")]
        public async Task ModHelpAsync([Remainder]string inputCommand = null)
        {
            if (!string.IsNullOrEmpty(inputCommand))
            {
                await UserHelpHandler.DisplayCommandHelpMenu(Context, "elevated " + inputCommand);
            }
            else
            {
                await UserHelpHandler.DisplayModerationHelpMenu(Context);
            }
        }
    }
}