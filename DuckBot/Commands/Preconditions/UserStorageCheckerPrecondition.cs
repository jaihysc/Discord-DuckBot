using Discord.Commands;
using DuckBot.UserActions;
using System;
using System.IO;
using System.Threading.Tasks;

namespace DuckBot.Commands.Preconditions
{
    class UserStorageCheckerPrecondition : PreconditionAttribute
    {
        // Override the CheckPermissions method
        public async override Task<PreconditionResult> CheckPermissions(ICommandContext context, CommandInfo command, IServiceProvider _services)
        {
            //Create txt user credit entry if user does not exist
            if (!File.Exists(TaskMethods.GetFileLocation(@"\UserStorage") + @"\" + context.Message.Author.Id + ".xml"))
            {
                //Create user profile
                UserXmlDataStorage.CreateNewUserXmlEntry(context as SocketCommandContext);
            }

            return PreconditionResult.FromSuccess();
        }
    }
}
