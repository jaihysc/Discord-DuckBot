using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DuckBot.Commands.Preconditions
{
    //Blacklist precondition
    public class BlacklistedUsersPrecondition : PreconditionAttribute
    {
        // Override the CheckPermissions method
        public async override Task<PreconditionResult> CheckPermissions(ICommandContext context, CommandInfo command, IServiceProvider _services)
        {
            //
            // If this command was executed by predefined users, return a failure
            List<ulong> blacklistedUsers = new List<ulong>();

            //---START Blacklist entry below
            blacklistedUsers.Add(423656753423581204);
           // blacklistedUsers.Add(387953113585418240);
            //---END blacklist entry

            //Test if user is blacklisted
            bool userIsBlackListed = false;
            foreach (var user in blacklistedUsers)
            {
                if (context.Message.Author.Id == user)
                {
                    userIsBlackListed = true;
                }
            }

            if (userIsBlackListed == false)
            {
                return PreconditionResult.FromSuccess();
            }
            else
            {
                return PreconditionResult.FromError("You have been blocked from using this command.");
            }

        }
    }
}
