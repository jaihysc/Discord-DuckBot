using Discord;
using Discord.Commands;
using Discord.WebSocket;
using DuckBot_ClassLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DuckBot.Modules.UserActions
{
    public class UserHelpHandler : ModuleBase<SocketCommandContext>
    {
        public static async Task DisplayHelpMenu(SocketCommandContext Context)
        {
            //https://leovoel.github.io/embed-visualizer/
            var builder = new EmbedBuilder()
                .WithDescription(" For a detailed guide on the usage of Duck, please check the [wiki](https://github.com/jaihysc/Discord-DuckBot/wiki/Main). \n \n Prefix: `.d`")
                .WithColor(new Color(253, 184, 20))
                .WithFooter(footer => {
                    footer
                        .WithText("To check command usage, type .d help <command> // Sent by " + Context.Message.Author.ToString());
                })
                .WithAuthor(author => {
                    author
                        .WithName("Duck Help")
                        .WithIconUrl("https://ubisafe.org/images/duck-transparent-jpeg-5.png");
                })
                .AddField("Currency Commands", "`balance` `daily` `debt` `borrow` `return` `moneyTransfer` ")
                .AddField("Game Commands", "`Prefix: game` | `slot`")
                .AddField("Stock Commands", "`Prefix: stock` | `portfolio` `market` `buy` `sell`");

            var embed = builder.Build();

            await Context.Message.Channel.SendMessageAsync(" ", embed: embed).ConfigureAwait(false);
        }

        public class HelpMenuCommands
        {
            public List<HelpMenuCommandEntry> CommandHelpEntry { get; set; }
        }
        public class HelpMenuCommandEntry
        {
            public string Command { get; set; }
            public string Description { get; set; }
            public string Usage { get; set; }
        }
        public static async Task DisplayCommandHelpMenu(SocketCommandContext Context, string inputCommand)
        {
            //Get command help list from storage
            var commandHelpDefinitionStorage = XmlManager.FromXmlFile<HelpMenuCommands>(CoreMethod.GetFileLocation(@"CommandHelpDescription.xml"));

            //Create a boolean to warn user that command does not exist if false
            bool commandHelpDefinitionExists = false;

            //Search commandHelpDefinitionStorage for command definition
            foreach (var commandHelpDefinition in commandHelpDefinitionStorage.CommandHelpEntry)
            {
                if (commandHelpDefinition.Command == inputCommand)
                {
                    commandHelpDefinitionExists = true;

                    var builder = new EmbedBuilder()
                    .WithDescription($"{commandHelpDefinition.Description} \n \n **Usage:** `{commandHelpDefinition.Usage}`")
                    .WithColor(new Color(253, 88, 20))
                    .WithFooter(footer =>
                    {
                        footer
                            .WithText("Sent by " + Context.Message.Author.ToString());
                    })
                    .WithAuthor(author =>
                    {
                        author
                            .WithName("Duck Help - " + inputCommand)
                            .WithIconUrl("https://ubisafe.org/images/duck-transparent-jpeg-5.png");
                    });

                    var embed = builder.Build();

                    await Context.Message.Channel.SendMessageAsync(" ", embed: embed).ConfigureAwait(false);
                }
            }

            //Send warning if command definition could not be found
            if (commandHelpDefinitionExists == false)
            {
                await Context.Channel.SendMessageAsync($"Command **{inputCommand}** could not be found");
            }
        }
    }
}
