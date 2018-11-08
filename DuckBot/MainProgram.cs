using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using DuckBot.Commands.Preconditions;
using DuckBot.Finance;
using DuckBot.Finance.ServiceThreads;
using DuckBot.UserActions;
using DuckBot_ClassLibrary;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace DuckBot
{
    public class MainProgram
    {
        public static string rootLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        public static void Main(string[] args)
        {
            //Declearation
            HelperMethod.DeclareUpdateTimeContainer("--Last Update Time--");
            CoreMethod.DeclareRootLocation(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));

            //Continously running threads declearation
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

        public static bool _stopThreads = false;

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
            catch (Exception) { Console.WriteLine("Unable to initialize!"); }

            await _commands.AddModulesAsync(Assembly.GetEntryAssembly());
            await _client.SetGameAsync("Use .d");

            //
            //Event handlers
            //

            //Update Stocks
            //Stock updater is currently inoperational due to high memory usage
            //To use, comment out task.delay
            /*
            await Task.Run(async () => {
                UserStocksHandler.UpdateUserStocks();
            });
            */

            //Log user / console messages
            _client.Log += EventLogger.Log;
            _client.MessageReceived += EventLogger.LogUserMessage;

            //Messaged received
            _client.MessageReceived += MessageReceived;

            //User joined
            _client.UserJoined += UserJoinHandler.DisplayUserGenderChoice;
            //_client.ReactionAdded += UserJoinHandler.GenderReactionInput();

            //Handles command on message received event
            _client.MessageReceived += HandleCommandAsync;

            //All commands before this
            await Task.Delay(-1);
            
        }

        public async Task HandleCommandAsync(SocketMessage messageParam)
        {
            // Don't process the command if it was a system message, if sender is bot or ID is not ME!
            SocketUserMessage message = messageParam as SocketUserMessage;
            if (message == null) return;

            if (message.Author.IsBot) return;

            //List of people who can use the bot
            //if (message.Author.Id != 285266023475838976 
                //message.Author.Id != 257225194391732237
                //) return;

            //if command is not sent in my beautiful dedicated channel
            //if (message.Channel.Id != 504371769738526752) return;

            //integer to determine when commands start
            int argPos = 0;

            if (!(message.HasStringPrefix(".d ", ref argPos) ||
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

                await channel.SendMessageAsync($"[ERROR] `{message}`  >|  {result.ErrorReason}");
            }

            //Logs command if successful
            if (result.IsSuccess)
            {
                var guild = _client.GetGuild(384492615745142784);
                var channel = guild.GetTextChannel(504375404547801138);

                await channel.SendMessageAsync($"[Log] `{message}`  >|  {result.ToString()}");
            }
            
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
