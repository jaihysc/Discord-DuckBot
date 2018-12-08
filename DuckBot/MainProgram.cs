using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using DuckBot.Core;
using DuckBot.Modules.CsgoCaseUnboxing;
using DuckBot.Modules.Finance;
using DuckBot.Modules.Finance.ServiceThreads;
using DuckBot.Modules.Moderation;
using DuckBot.Modules.UserActions;
using DuckBot_ClassLibrary;
using DuckBot_ClassLibrary.Modules;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace DuckBot
{
    public class MainProgram
    {
        public static string rootLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        public static bool _stopThreads = false;
        public static string botCommandPrefix = ".d";


        //Setup
        public static void Main(string[] args)
        {
            //Injection
            //This will be depreciated soon
            CoreMethod.DeclareRootLocation(rootLocation);

            //Runs setup if config files are not present
            if (!File.Exists(rootLocation + @"\Paths.txt"))
            {
                SetupManager.GenerateConfigFile();
            }

            //Continously running threads
            Thread updateUserBankingInterest = new Thread(new ThreadStart(UserBankingInterestUpdater.UpdateUserDebtInterest));
            Thread updateUserMarketStocks = new Thread(new ThreadStart(UserMarketStocksUpdater.UpdateMarketStocks));

            //Start
            updateUserBankingInterest.Start();
            updateUserMarketStocks.Start();

            new MainProgram().MainAsync().GetAwaiter().GetResult();


        }

        public DiscordSocketClient _client;
        public CommandService _commands;
        public IServiceProvider _services;

        //Main
        public async Task MainAsync()
        {
            var _config = new DiscordSocketConfig { MessageCacheSize = 100 };

            _client = new DiscordSocketClient(_config);
            _commands = new CommandService();
            _services = new ServiceCollection()
                .AddSingleton(_client)
                .AddSingleton<InteractiveService>()

                //BuildsServiceProvider
                .BuildServiceProvider();

            //Bot init
            try
            {
                //Get token
                var configLocations = File.ReadAllLines(rootLocation + @"\Paths.txt");
                var tokenLocation = configLocations.Where(p => p.Contains("BotToken.txt")).ToArray();

                string token = "";
                foreach (var item in tokenLocation)
                {
                    var tokenFile = File.ReadAllLines(item);
                    foreach (var item2 in tokenFile)
                    {
                        token = item2;
                    }
                }

                //Connect to discord
                await _client.LoginAsync(TokenType.Bot, token);
                await _client.StartAsync();

            }
            catch (Exception)
            {
                throw new Exception("Unable to initialize!");
            }

            await _commands.AddModulesAsync(Assembly.GetEntryAssembly());

            //Set help text playing
            await _client.SetGameAsync($"Use {botCommandPrefix} help");

            //
            //Event handlers
            //

            //Log user / console messages
            _client.Log += EventLogger.Log;
            _client.MessageReceived += EventLogger.LogUserMessage;

            //Message received
            _client.MessageReceived += MessageReceived;
            _client.MessageReceived += ModerationManager.ModerationManagerMessageReceived;

            //User joined
            _client.UserJoined += UserJoinHandler.DisplayUserGenderChoice;
            //_client.ReactionAdded += UserJoinHandler.GenderReactionInput();

            //Handles command on message received event
            _client.MessageReceived += HandleCommandAsync;

            //All commands before this
            await Task.Delay(-1);

        }

        //Command Handler
        public async Task HandleCommandAsync(SocketMessage messageParam)
        {
            // Don't process the command if it was a system message, if sender is bot or ID is not ME!
            SocketUserMessage message = messageParam as SocketUserMessage;
            if (message == null) return;

            if (message.Author.IsBot) return;

            //if command is not sent in my beautiful dedicated channel
            //if (message.Channel.Id != 504371769738526752) return;

            //integer to determine when commands start
            int argPos = 0;

            if (!(message.HasStringPrefix(MainProgram.botCommandPrefix + " ", ref argPos) ||
                message.Author.IsBot))
                return;

            var context = new SocketCommandContext(_client, message);
            var result = await _commands.ExecuteAsync(context: context, argPos: argPos, services: _services);

            //COMMAND LOGGING
            // Inform the user if the command fails

            if (!result.IsSuccess)
            {
                var guild = _client.GetGuild(384492615745142784);
                var channel = guild.GetTextChannel(504375404547801138);

                if (result.Error == CommandError.UnknownCommand)
                {
                    //Find similar commands
                    var commandHelpDefinitionStorage = XmlManager.FromXmlFile<UserHelpHandler.HelpMenuCommands>(CoreMethod.GetFileLocation(@"CommandHelpDescription.xml"));
                    string similarItemsString = UserHelpHandler.FindSimilarCommands(
                        commandHelpDefinitionStorage.CommandHelpEntry.Select(i => i.CommandName).ToList(), 
                        message.ToString().Substring(botCommandPrefix.Length + 1));

                    //If no similar matches are found, send nothing
                    if (string.IsNullOrEmpty(similarItemsString))
                    {
                        await context.Channel.SendMessageAsync("Invalid command, use `.d help` for a list of commands");
                    }
                    //If similar matches are found, send suggestions
                    else
                    {
                        await context.Channel.SendMessageAsync($"Invalid command, use `.d help` for a list of commands. Did you mean: \n {similarItemsString}");
                    }
                    
                }
                else if (result.Error == CommandError.BadArgCount)
                {
                    await context.Channel.SendMessageAsync($"Invalid command usage, use `.d help <command>` for correct command usage");
                }
                else if (result.Error == CommandError.UnmetPrecondition)
                {
                    //await context.Channel.SendMessageAsync($"Woah, Slow down (Ratelimited)");
                }
                else
                {
                    await channel.SendMessageAsync($"[ERROR] **{message.Author.ToString()}** `{message}`  >|  {result.ErrorReason}");
                }
            }

            /*
            //Logs command if successful
            if (result.IsSuccess)
            {
                var guild = _client.GetGuild(384492615745142784);
                var channel = guild.GetTextChannel(504375404547801138);

                await channel.SendMessageAsync($"[Log] `{message}`  >|  {result.ToString()}");
            }
            */

        }

        private async Task MessageReceived(SocketMessage message)
        {
            //Message detection
            CultureInfo culture = new CultureInfo("en-CA", false);

            await ProhibitedWordsChecker.ProhibitedWordsHandler(message, rootLocation);

            if (culture.CompareInfo.IndexOf(message.Content, "rule34", CompareOptions.IgnoreCase) >= 0 && message.Author.IsBot != true)
            {
                await message.Channel.SendMessageAsync("Woah hey hey hey, watch it! You have to be 18+ to use that command and I guarantee you that you aren't.");
            }

            if (culture.CompareInfo.IndexOf(message.Content, "->fish", CompareOptions.IgnoreCase) >= 0 && message.Author.IsBot != true)
            {
                await message.Channel.SendMessageAsync("Hey! How dare you fish in my pond, no regard for our species and our survival");
            }

        }
    }
}
