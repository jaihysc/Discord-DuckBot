using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace DuckBot.Commands.Preconditions
{
    //Owner only precondition
    public class RequireWhitelistedUsers : PreconditionAttribute
    {
        // Override the CheckPermissions method
        public async override Task<PreconditionResult> CheckPermissions(ICommandContext context, CommandInfo command, IServiceProvider _services)
        {
            // Get the client via Depedency Injection
            var client = _services.GetRequiredService<DiscordSocketClient>();

            // Get the ID of the bot's owner
            var appInfo = await client.GetApplicationInfoAsync().ConfigureAwait(false);
            var ownerId = appInfo.Owner.Id;

            // If this command was executed by owner or predefined users, return a success
            if (context.User.Id == ownerId ||
                context.User.Id == 387953113585418240)
                return PreconditionResult.FromSuccess();

            // Since it wasn't, fail
            else
                return PreconditionResult.FromError("You must be whitelisted by the great duck commander to use this command.");
        }
    }
}
