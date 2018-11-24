using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DuckBot.Modules.UserActions
{
    public class UserJoinHandler : ModuleBase<SocketCommandContext>
    {
        public static async Task DisplayUserGenderChoice(SocketGuildUser joinedUser)
        {
            var builder = new EmbedBuilder()
            .WithTitle("Welcome " + joinedUser.ToString())
            //.WithDescription(joinedUser.Mention)
            .WithColor(new Color(0x99BEBA))
            .AddField("As this is a sexist server, please select your gender below", "Use `.d gender boy` or Use `.d gender girl`");
            //.AddInlineField("<:thonkang:219069250692841473>", "these last two")

            var embed = builder.Build();

            var guild = joinedUser.Guild.GetTextChannel(389123701632663562);
            await guild.SendMessageAsync(joinedUser.Mention, embed: embed).ConfigureAwait(false);
        }
    }
}
