using Discord;
using Discord.Commands;
using DuckBot.Core;
using Discord.WebSocket;
using DuckBot.Models;
using DuckBot_ClassLibrary;
using DuckBot_ClassLibrary.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DuckBot.Modules.UserActions
{
    public class UserHelpHandler : ModuleBase<SocketCommandContext>
    {
        public static async Task DisplayHelpMenu(SocketCommandContext context)
        {
            string botCommandPrefix = CommandGuildPrefixManager.GetGuildCommandPrefix(context);

            //https://leovoel.github.io/embed-visualizer/
            var embedBuilder = new EmbedBuilder()
                .WithDescription($" For a detailed guide on the usage of Duck, please check the [wiki](https://github.com/jaihysc/Discord-DuckBot/wiki). \n \n Prefix: `{botCommandPrefix}`")
                .WithColor(new Color(253, 184, 20))
                .WithFooter(footer =>
                {
                    footer
                        .WithText("To check command usage, type .d help <command> // Use .d @help for moderation commands // Sent by " + context.Message.Author.ToString())
                        .WithIconUrl(context.Message.Author.GetAvatarUrl());
                })
                .WithAuthor(author =>
                {
                    author
                        .WithName("Duck Help")
                        .WithIconUrl("https://ubisafe.org/images/duck-transparent-jpeg-5.png");
                })
                .AddField("Currency Commands", "`balance` `daily` `debt` `borrow` `return` `moneyTransfer` `bankruptcy` ")
                .AddField("Game Commands", "`Prefix: game` | `slot`")
                .AddField("Stock Commands", "`Prefix: stock` | `portfolio` `market` `buy` `sell`")
                .AddField("Case Commands", "`Prefix: cs` | `open` `drop` `case` `inventory` `market` `buy` `sell` `info` ")
                .AddField("Role Commands", "`Prefix: role` | `list` `add` `remove`");

            var embed = embedBuilder.Build();

            await context.Message.Channel.SendMessageAsync(" ", embed: embed).ConfigureAwait(false);
        }

        public static async Task DisplayModerationHelpMenu(SocketCommandContext context)
        {
            string botCommandPrefix = CommandGuildPrefixManager.GetGuildCommandPrefix(context);

            var embedBuilder = new EmbedBuilder()
                .WithDescription(" For a detailed guide on the usage of Duck, please check the [wiki](https://github.com/jaihysc/Discord-DuckBot/wiki/Main). \n \n Prefix: `.d elevated` \n Requires permission `Administrator`")
                .WithColor(new Color(252, 144, 0))
                .WithFooter(footer =>
                {
                    footer
                        .WithText($"To check command usage, type `{botCommandPrefix}` @help <command> // Use `{botCommandPrefix}` help for standard commands // Sent by " + context.Message.Author.ToString())
                        .WithIconUrl(context.Message.Author.GetAvatarUrl());
                })
                .WithAuthor(author =>
                {
                    author
                        .WithName("Duck Moderation Help")
                        .WithIconUrl("https://ubisafe.org/images/duck-transparent-jpeg-5.png");
                })
                .AddField("Channel Commands", "`clean`")
                .AddField("Role Commands", "`Prefix: role` | `add` `remove`");

            var embed = embedBuilder.Build();

            await context.Message.Channel.SendMessageAsync(" ", embed: embed).ConfigureAwait(false);
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
                if (commandHelpDefinition.CommandName == inputCommand)
                {
                    commandHelpDefinitionExists = true;

                    var embedBuilder = new EmbedBuilder()
                    .WithDescription($"**{commandHelpDefinition.CommandDescription}**")
                    .WithColor(new Color(253, 88, 20))
                    .WithFooter(footer =>
                    {
                        footer
                            .WithText("Sent by " + Context.Message.Author.ToString())
                            .WithIconUrl(Context.Message.Author.GetAvatarUrl());
                    })
                    .WithAuthor(author =>
                    {
                        author
                            .WithName("Duck Help - " + inputCommand)
                            .WithIconUrl("https://ubisafe.org/images/duck-transparent-jpeg-5.png");
                    });

                    if (!string.IsNullOrEmpty(commandHelpDefinition.CommandRequiredPermissions))
                    {
                        embedBuilder.AddField("Permissions required", $"`{commandHelpDefinition.CommandRequiredPermissions}`");
                    }

                    embedBuilder.AddField("Usage", $"`{commandHelpDefinition.CommandUsage}`");

                    if (!string.IsNullOrEmpty(commandHelpDefinition.CommandUsageDefinition))
                    {
                        embedBuilder.AddField("Definitions", commandHelpDefinition.CommandUsageDefinition);
                    }

                    var embed = embedBuilder.Build();

                    await Context.Message.Channel.SendMessageAsync(" ", embed: embed).ConfigureAwait(false);
                }
            }

            //Send warning if command definition could not be found
            if (commandHelpDefinitionExists == false)
            {
                string similarItemsString = FindSimilarCommands(commandHelpDefinitionStorage.CommandHelpEntry.Select(i => i.CommandName).ToList(), inputCommand);

                //If no similar matches are found, send nothing
                if (string.IsNullOrEmpty(similarItemsString))
                {
                    await Context.Channel.SendMessageAsync($"Command **{inputCommand}** could not be found");
                }
                //If similar matches are found, send suggestions
                else
                {
                    await Context.Channel.SendMessageAsync($"Command **{inputCommand}** could not be found. Did you mean: \n {similarItemsString}");
                }

            }
        }

        /// <summary>
        /// Try to find similar help items based on input
        /// </summary>
        /// <param name="storedCommands">String list of stored strings</param>
        /// <param name="inputCommand">Input string to check</param>
        /// <returns></returns>
        public static string FindSimilarCommands(List<string> storedCommands, string inputCommand, int fuzzyIndex=6)
        {
            //Filter out command names to string
            string similarItemsString = "";

            foreach (var item in storedCommands)
            {
                //If fuzzy search difference is less than 6 or if storedCommand contains inputCommand
                if (FuzzySearchManager.Compute(item.ToLower(), inputCommand.ToLower()) < fuzzyIndex ||
                    item.ToLower().Contains(inputCommand.ToLower()))
                {
                    //Concat items in list together
                    similarItemsString = string.Concat(similarItemsString, "\n", item);
                }

            }

            return similarItemsString;
        }
    }
}


