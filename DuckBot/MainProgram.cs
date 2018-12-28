using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using DuckBot.Core;
using DuckBot.Models;
using DuckBot.Modules.Csgo;
using DuckBot.Modules.Finance;
using DuckBot.Modules.Finance.ServiceThreads;
using DuckBot.Modules.Moderation;
using DuckBot.Modules.UserActions;
using DuckBot_ClassLibrary;
using DuckBot_ClassLibrary.Modules;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics;
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
        public static bool _stopThreads = false;
        public static string botCommandPrefix = ".d";

        private static Stopwatch stopwatch = new Stopwatch();

        //Setup
        public static void Main(string[] args)
        {
            //Injection
            stopwatch.Start();
            EventLogger.LogMessage("Hello World! - Beginning startup");


            //Runs setup if config files are not present
            if (!File.Exists(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\Paths.txt"))
            {
                SetupManager.GenerateConfigFile();
            }

            //Continously running threads
            Thread updateUserBankingInterest = new Thread(new ThreadStart(UserBankingInterestUpdater.UpdateUserDebtInterest));
            Thread updateUserMarketStocks = new Thread(new ThreadStart(UserMarketStocksUpdater.UpdateMarketStocks));
            //Start threads
            updateUserBankingInterest.Start();
            updateUserMarketStocks.Start();

            //Setup
            CsgoDataHandler.GetRootWeaponSkin();

            //Main
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
                string token = File.ReadAllLines(CoreMethod.GetFileLocation("BotToken.txt")).FirstOrDefault();

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


            stopwatch.Stop();
            EventLogger.LogMessage($"Ready! - Took {stopwatch.ElapsedMilliseconds} milliseconds");

            //
            //Event handlers
            //

            //Log user / console messages
            _client.Log += EventLogger.Log;
            _client.MessageReceived += EventLogger.LogUserMessage;

            //Message received
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
                    var commandHelpDefinitionStorage = XmlManager.FromXmlFile<HelpMenuCommands>(CoreMethod.GetFileLocation(@"CommandHelpDescription.xml"));
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
    }
}
