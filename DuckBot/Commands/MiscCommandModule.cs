﻿using Discord;
using Discord.Commands;
using DuckBot.Finance;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DuckBot.Commands
{
    class MiscCommandModule : ModuleBase<SocketCommandContext>
    {
        //Genders
        [Command("gender boy")]
        public async Task SetGenderMaleAsync()
        {
            var user = Context.User;
            var role = Context.Guild.Roles.FirstOrDefault(x => x.Id == ConfigValues.boyRoleId);
            var removeRole = Context.Guild.Roles.FirstOrDefault(x => x.Id == ConfigValues.girlRole2Id);

            await (user as IGuildUser).AddRoleAsync(role);
            await (user as IGuildUser).RemoveRoleAsync(removeRole);
        }
        [Command("gender girl")]
        public async Task SetGenderFemaleAsync()
        {
            var user = Context.User;
            var role = Context.Guild.Roles.FirstOrDefault(x => x.Id == ConfigValues.girlRole2Id);
            var removeRole = Context.Guild.Roles.FirstOrDefault(x => x.Id == ConfigValues.boyRoleId);

            await (user as IGuildUser).AddRoleAsync(role);
            await (user as IGuildUser).RemoveRoleAsync(removeRole);
        }
    }
}
