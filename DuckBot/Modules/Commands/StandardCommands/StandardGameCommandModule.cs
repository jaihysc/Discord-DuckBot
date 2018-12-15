using Discord.Commands;
using DuckBot.Modules.Commands.Preconditions;
using DuckBot.Modules.CsgoCaseUnboxing;
using DuckBot.Modules.UserFinance;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DuckBot.Modules.Commands.StandardCommands
{
    [Ratelimit(1, 5, Measure.Seconds)]
    [BlacklistedUsersPrecondition]
    [UserStorageCheckerPrecondition]
    [Group("game")]
    [Alias("g")]
    public class StandardGameCommandModule : ModuleBase<SocketCommandContext>
    {
        //Gambling
        [Command("slot", RunMode = RunMode.Async)]
        [Alias("s")]
        public async Task PlaySlotAsync(long gambleAmount)
        {
            await UserGamblingHandler.UserGambling(Context, Context.Message, gambleAmount);
        }
    }
}
