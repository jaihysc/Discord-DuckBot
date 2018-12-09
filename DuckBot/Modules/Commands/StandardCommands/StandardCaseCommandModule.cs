using Discord.Commands;
using DuckBot.Modules.Commands.Preconditions;
using DuckBot.Modules.CsgoCaseUnboxing;
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
    [Group("case")]
    [Alias("c")]
    public class StandardCaseCommandModule : ModuleBase<SocketCommandContext>
    {
        [Command("open")]
        [Alias("o")]
        public async Task OpenCaseAsync()
        {
            await UnboxingHandler.OpenCase(Context);
        }
    }
}
