﻿using Discord;
using Discord.Commands;
using DuckBot.Commands.Preconditions;
using DuckBot.Finance;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DuckBot.Commands
{
    [BlacklistedUsersPrecondition]
    [UserStorageCheckerPrecondition]
    public class ElevatedCommandModule : ModuleBase<SocketCommandContext>
    {
        [Group("elevated")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public class CrossGuild : ModuleBase<SocketCommandContext>
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

            //Modify assigning role names
            [Command("editRole boy")]
            public async Task ChangeGenderMaleRoleAsync([Remainder]ulong roleID)
            {
                ConfigValues.boyRoleId = roleID;
            }
            [Command("editRole girl")]
            public async Task ChangeGenderFemaleRoleAsync([Remainder]ulong roleID)
            {
                ConfigValues.girlRole2Id = roleID;
            }
        }
    }
}
