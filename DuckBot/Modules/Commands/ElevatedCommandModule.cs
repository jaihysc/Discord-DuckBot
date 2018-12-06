using Discord;
using Discord.Commands;
using DuckBot.Core;
using DuckBot.Modules.Commands.Preconditions;
using DuckBot.Modules.Finance;
using DuckBot.Modules.Moderation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DuckBot.Modules.Commands
{
    [UserStorageCheckerPrecondition]
    [BlacklistedUsersPrecondition]
    [UserStorageCheckerPrecondition]
    [RequireUserPermission(GuildPermission.Administrator)]
    public class ElevatedCommandModule : ModuleBase<SocketCommandContext>
    {
        [Group("elevated")]
        [Alias("e")]
        public class Elevated : ModuleBase<SocketCommandContext>
        {
            [Command("clean")]
            public async Task CleanMessagesAsync(int messageAmount, string deleteAuthorTarget = null)
            {
                //Get messages of n amount
                var messages = await Context.Channel.GetMessagesAsync(messageAmount).Flatten();

                //If delete author target is not assigned, delete n messages, if assigned, delete n messages from author
                if (!string.IsNullOrEmpty(deleteAuthorTarget))
                {
                    ulong deleteAuthorID = MentionUtils.ParseUser(deleteAuthorTarget);
                    var selectedMessages = messages.Where(p => p.Author.Id == deleteAuthorID);
                    await Context.Channel.DeleteMessagesAsync(selectedMessages);
                }
                else
                {
                    await Context.Channel.DeleteMessagesAsync(messages);
                }
            }

            [Group("role")]
            [Alias("r")]
            public class Role : ModuleBase<SocketCommandContext>
            {

                [Command("add")]
                public async Task AddBotManagedRoleAsync(string roleStringInput, [Remainder]string roleName)
                {
                    //Parse roleId from mention
                    try
                    {
                        ulong roleID = MentionUtils.ParseRole(roleStringInput);
                        GuildRolesManager.AddGuildRole(Context.Guild.Id, roleName, roleID);
                    }
                    catch (Exception)
                    {
                        await Context.Channel.SendMessageAsync($"Error in finding role, role {roleStringInput} does not exist");
                    }
                }

                [Command("remove")]
                public async Task RemoveBotManagedRoleAsync(string roleStringInput, [Remainder]string roleName)
                {
                    //Parse roleId from mention
                    try
                    {
                        ulong roleID = MentionUtils.ParseRole(roleStringInput);
                        GuildRolesManager.RemoveGuildRole(Context.Guild.Id, roleName, roleID);
                    }
                    catch (Exception ex)
                    {
                        await Context.Channel.SendMessageAsync($"Error in finding role, role {roleStringInput + ex.Message} does not exist");
                    }
                }
            }
        }
    }
}
