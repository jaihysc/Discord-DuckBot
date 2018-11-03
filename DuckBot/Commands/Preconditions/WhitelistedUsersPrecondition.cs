using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DuckBot.Commands.Preconditions
{
    //Whitelist precondition
    public class WhitelistedUsersPrecondition : PreconditionAttribute
    {
        // Override the CheckPermissions method
        public async override Task<PreconditionResult> CheckPermissions(ICommandContext context, CommandInfo command, IServiceProvider _services)
        {
            // Get the client via Depedency Injection
            var client = _services.GetRequiredService<DiscordSocketClient>();

            // Get the ID of the bot's owner
            var appInfo = await client.GetApplicationInfoAsync().ConfigureAwait(false);
            var ownerId = appInfo.Owner.Id;

            //
            // If this command was executed by predefined users, return a failure
            List<ulong> whitelistedUsers = new List<ulong>();

            //---START Blacklist entry below
            whitelistedUsers.Add(ownerId);
            whitelistedUsers.Add(387953113585418240);
            //---END blacklist entry

            //Test if user is blacklisted
            bool userIsWhiteListed = false;
            foreach (var user in whitelistedUsers)
            {
                if (context.Message.Author.Id == user)
                {
                    userIsWhiteListed = true;
                }
            }

            if (userIsWhiteListed == false)
            {
                return PreconditionResult.FromError("You must be whitelisted by the great duck commander to use this command.");
            }
            else
            {
                return PreconditionResult.FromSuccess();
            }

        }
    }
}
