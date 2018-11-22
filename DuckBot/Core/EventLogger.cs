using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace DuckBot.Core
{
    class EventLogger : ModuleBase<SocketCommandContext>
    {
        public static Task Log(LogMessage message)
        {
            //Logs server messages to console
            switch (message.Severity)
            {
                case LogSeverity.Critical:
                case LogSeverity.Error:
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
                case LogSeverity.Warning:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    break;
                case LogSeverity.Info:
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    break;
                case LogSeverity.Verbose:
                case LogSeverity.Debug:
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    break;
            }
            Console.WriteLine($"{DateTime.Now,-19} [{message.Severity,8}] {message.Source}: {message.Message}");
            Console.ForegroundColor = ConsoleColor.White;
            return Task.CompletedTask;
        }

        public static Task LogUserMessage(SocketMessage msg)
        {
            //Log user message to file
            var chnl = msg.Channel as SocketGuildChannel;
            var cc = Console.BackgroundColor;

            try
            {
                Console.Write($"{DateTime.Now,-19} [    Log] {chnl.Guild.Name} ||  {msg.Channel} - {msg.Author}: ");
            }
            catch (Exception)
            {
                Console.Write($"{DateTime.Now,-19} [    Log] Direct Message >| {msg.Channel} - {msg.Author}: ");
            }

            Console.BackgroundColor = ConsoleColor.DarkGray;
            Console.WriteLine(msg.ToString());

            var configLocations = File.ReadAllLines(MainProgram.rootLocation + @"\Paths.txt");
            var logLocation = configLocations.Where(p => p.Contains("BotOutputLog.txt")).ToArray();

            foreach (var item in logLocation)
            {
                try
                {
                    using (System.IO.StreamWriter file =
                    new System.IO.StreamWriter(item, true))
                    {
                        file.WriteLine($"{DateTime.Now,-19} [    Log] {chnl.Guild.Name} || {msg.Channel} - {msg.Author}: {msg.ToString()}");
                    }
                }
                catch (Exception)
                {
                    try
                    {
                        using (System.IO.StreamWriter file =
                        new System.IO.StreamWriter(item, true))
                        {
                            file.WriteLine($"{DateTime.Now,-19} [    Log] Direct Message >| {msg.Channel} - {msg.Author}: {msg.ToString()}");
                        }
                    }
                    catch (Exception)
                    {
                        Console.WriteLine("Unable to write to log file!");
                    }
                }
            }
            Console.BackgroundColor = cc;
            return Task.CompletedTask;
        }

    }
}
