using Discord;
using Discord.Commands;
using DuckBot.Core;
using DuckBot.Modules.Finance;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DuckBot.Modules.Commands
{
    class MiscCommandModule : ModuleBase<SocketCommandContext>
    {
        //Genders
        [Command("gender boy")]
        public async Task SetGenderMaleAsync()
        {
            var user = Context.User;
            var role = Context.Guild.Roles.FirstOrDefault(x => x.Id == ulong.Parse(SettingsManager.RetrieveFromConfigFile("boyRoleId")));
            var removeRole = Context.Guild.Roles.FirstOrDefault(x => x.Id == ulong.Parse(SettingsManager.RetrieveFromConfigFile("girlRoleId")));

            await (user as IGuildUser).AddRoleAsync(role);
            await (user as IGuildUser).RemoveRoleAsync(removeRole);
        }
        [Command("gender girl")]
        public async Task SetGenderFemaleAsync()
        {
            var user = Context.User;
            var role = Context.Guild.Roles.FirstOrDefault(x => x.Id == ulong.Parse(SettingsManager.RetrieveFromConfigFile("girlRoleId")));
            var removeRole = Context.Guild.Roles.FirstOrDefault(x => x.Id == ulong.Parse(SettingsManager.RetrieveFromConfigFile("boyRoleId")));

            await (user as IGuildUser).AddRoleAsync(role);
            await (user as IGuildUser).RemoveRoleAsync(removeRole);
        }
    }
}
