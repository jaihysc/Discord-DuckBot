﻿using Discord;
using Discord.Commands;
using DuckBot.Core;
using DuckBot.Modules.Finance;
using DuckBot.Modules.Moderation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DuckBot.Modules.Commands.StandardCommands
{
    [Group("role")]
    [Alias("r")]
    public class StandardRoleCommandModule : ModuleBase<SocketCommandContext>
    {
        //Roles
        [Command("list")]
        public async Task GetRoleListAsync()
        {
            var returnGuildRoles = GuildRolesManager.GetGuildRoles(Context.Guild.Id);

            //User stock list
            List<string> guildRoleNameList = new List<string>();

            //Get user portfolio
            var embedBuilder = new EmbedBuilder()
                .WithColor(new Color(40, 144, 175))
                .WithFooter(footer =>
                {
                    footer
                        .WithText("Sent by " + Context.Message.Author.ToString());
                })
                .WithAuthor(author =>
                {
                    author
                        .WithName("Guild Roles - " + Context.Guild.ToString())
                        .WithIconUrl("https://upload.wikimedia.org/wikipedia/commons/thumb/3/34/Talkicon.svg/480px-Talkicon.svg.png");
                });

            //Add roles to embed
            foreach (var role in returnGuildRoles)
            {
                guildRoleNameList.Add($"**{role.RoleName}**");
            }

            //Join guild role names from list
            string joinedGuildRoleNameList = string.Join(" \n ", guildRoleNameList);

            //If guild has no set roles, add invisible character
            if (string.IsNullOrEmpty(joinedGuildRoleNameList))
            {
                embedBuilder.AddInlineField("Role Name", "\u200b");
            }
            //If guild has roles, add roles
            else
            {
                embedBuilder.AddInlineField("Role Name", joinedGuildRoleNameList);
            }

            //Send user stock portfolio
            var embed = embedBuilder.Build();

            await Context.Message.Channel.SendMessageAsync(" ", embed: embed).ConfigureAwait(false);
        }
        [Command("add")]
        public async Task AddRoleFromListAsync([Remainder]string roleName)
        {
            //Get roles for guild
            var returnGuildRoles = GuildRolesManager.GetGuildRoles(Context.Guild.Id);

            //Filter role input to one user selected
            var role = returnGuildRoles.FirstOrDefault(x => x.GuildID == Context.Guild.Id && x.RoleName == roleName);

            //Get role from guild
            var selectedRole = Context.Guild.Roles.FirstOrDefault(x => x.Id == role.GuildRoleID);

            //Add role to user
            await (Context.User as IGuildUser).AddRoleAsync(selectedRole);
        }
        [Command("remove")]
        public async Task RemoveRoleFromListAsync([Remainder]string roleName)
        {
            //Get roles for guild
            var returnGuildRoles = GuildRolesManager.GetGuildRoles(Context.Guild.Id);

            //Filter role input to one user selected
            var role = returnGuildRoles.FirstOrDefault(x => x.GuildID == Context.Guild.Id && x.RoleName == roleName);

            //Get role from guild
            var selectedRole = Context.Guild.Roles.FirstOrDefault(x => x.Id == role.GuildRoleID);

            //Add role to user
            await (Context.User as IGuildUser).RemoveRoleAsync(selectedRole);
        }
    }
}
