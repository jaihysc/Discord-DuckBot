using Discord;
using Discord.Commands;
using Discord.WebSocket;
using DuckBot.Modules.Finance;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DuckBot.Modules.Commands.Preconditions;
using DuckBot.Modules.Finance.CurrencyManager;
using DuckBot.Core;

namespace DuckBot.Modules.Commands
{
    public class RestrictedCommandModule : ModuleBase<SocketCommandContext>
    {
        [Group("crossGuild")]
        [Alias("c")]
        [WhitelistedUsersPrecondition]
        [UserStorageCheckerPrecondition]
        public class CrossGuild : ModuleBase<SocketCommandContext>
        {
            [Group("elevated")]
            [Alias("e")]
            public class Elevated : ModuleBase<SocketCommandContext>
            {
                [Command("clean")]
                public async Task CleanMessagesAsync(int messageAmount, ulong deleteAuthorTarget = 0)
                {
                    var messages = await Context.Channel.GetMessagesAsync(messageAmount).Flatten();

                    if (deleteAuthorTarget != 0)
                    {
                        var selectedMessages = messages.Where(p => p.Author.Id == deleteAuthorTarget);
                        await Context.Channel.DeleteMessagesAsync(selectedMessages);
                    }
                    else
                    {
                        await Context.Channel.DeleteMessagesAsync(messages);
                    }
                }
            }

            [Command("ban")]
            public async Task BanUserAsync(ulong guildID, ulong targetUserID)
            {
                var guild = Context.Client.GetGuild(guildID);

                await guild.AddBanAsync(targetUserID);
            }

            [Command("unban")]
            public async Task UnbanUserAsync(ulong guildID, ulong targetUserID)
            {
                var guild = Context.Client.GetGuild(guildID);

                await guild.RemoveBanAsync(targetUserID);
            }

            [Command("kick")]
            public async Task KickUserAsync(ulong guildID, ulong targetUserID)
            {
                var guild = Context.Client.GetGuild(guildID);
                var user = guild.GetUser(targetUserID);

                await user.KickAsync();
            }

            [Command("sendMessage")]
            public async Task SendMessageAsync(ulong guildID, ulong sendTargetChannelID, [Remainder]string message)
            {
                var guild = Context.Client.GetGuild(guildID);
                var sendChnl = guild.GetTextChannel(sendTargetChannelID);

                await sendChnl.SendMessageAsync(message);
            }

            [Command("dmMessage")]
            public async Task DirectMessageAsync(ulong guildID, ulong sendTargetUserID, [Remainder]string message)
            {
                var guild = Context.Client.GetGuild(guildID);
                var user = guild.GetUser(sendTargetUserID);

                await user.SendMessageAsync(message);
            }

            [Command("deleteMessageSingle")]
            public async Task DeleteMessageAsync(ulong guildID, ulong deleteTargetChannelID, ulong deleteTargetMessageID)
            {
                var guild = Context.Client.GetGuild(guildID);
                var chnl = guild.GetChannel(deleteTargetChannelID);
                var targetChnl = chnl.Guild.GetTextChannel(deleteTargetChannelID);
                var selectedChannelMessages = await targetChnl.GetMessagesAsync(1000).Flatten();
                var selectedMessage = selectedChannelMessages.Where(p => p.Id == deleteTargetMessageID);

                await targetChnl.DeleteMessagesAsync(selectedMessage);
            }

            [Command("changeNick")]
            public async Task ChangeNicknameAsync(ulong guildID, ulong sendTargetUserID, [Remainder]string nickname)
            {
                var guild = Context.Client.GetGuild(guildID);
                var user = guild.GetUser(sendTargetUserID);

                await user.ModifyAsync(p => p.Nickname = nickname);
            }

            [Command("printRoles")]
            public async Task PrintRolesAsync(ulong guildID)
            {
                string guildRoleMessage = "";

                var guild = Context.Client.GetGuild(guildID);
                var rolesName = guild.Roles.ToArray();
                var rolesId = guild.Roles.Select(x => x.Id);

                List<string> returnRoleListName = new List<string>();
                List<string> returnRoleListID = new List<string>();

                foreach (var item in rolesName)
                {
                    returnRoleListName.Add(item.ToString());
                }
                foreach (var item in rolesId)
                {
                    returnRoleListID.Add(item.ToString());
                }

                for (int i = 0; i < returnRoleListName.Count; i++)
                {
                    guildRoleMessage += " || " + returnRoleListName[i] + ", " + returnRoleListID[i];
                }

                await Context.Channel.SendMessageAsync(guildRoleMessage);
            }

            [Command("giveRoleNamed")]
            public async Task GiveRoleAsync(ulong guildID, ulong userID, [Remainder]string roleName)
            {
                var guild = Context.Client.GetGuild(guildID);
                var role = guild.Roles.FirstOrDefault(p => p.Name == roleName);

                var user = guild.GetUser(userID);
                await (user as IGuildUser).AddRoleAsync(role);
            }

            [Command("giveRole")]
            public async Task GiveRoleAsync(ulong guildID, ulong userID, ulong roleID)
            {
                var guild = Context.Client.GetGuild(guildID);
                var role = guild.GetRole(roleID);

                var user = guild.GetUser(userID);
                await (user as IGuildUser).AddRoleAsync(role);
            }

            [Command("removeRoleNamed")]
            public async Task RemoveRoleAsync(ulong guildID, ulong userID, [Remainder]string roleName)
            {
                var guild = Context.Client.GetGuild(guildID);
                var role = guild.Roles.FirstOrDefault(p => p.Name == roleName);

                var user = guild.GetUser(userID);
                await (user as IGuildUser).RemoveRoleAsync(role);
            }

            [Command("removeRole")]
            public async Task RemoveRoleAsync(ulong guildID, ulong userID, ulong roleID)
            {
                var guild = Context.Client.GetGuild(guildID);
                var role = guild.GetRole(roleID);

                var user = guild.GetUser(userID);
                await (user as IGuildUser).RemoveRoleAsync(role);
            }
        }
    }
}
