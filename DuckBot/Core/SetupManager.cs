using Discord.Commands;
using DuckBot_ClassLibrary;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DuckBot.Core
{
    public class SetupManager : ModuleBase<SocketCommandContext>
    {
        public static void GenerateConfigFile()
        {
            List<string> paths = new List<string>();

            //Get user input and generate config file
            Console.WriteLine("It appears this is your first startup of DuckBot");
            Console.WriteLine("This setup wizard will configure the settings of the bot");
            Console.WriteLine();
            Console.WriteLine("Press any key to continue...");
            Console.ReadLine();

            Console.WriteLine();
            Console.WriteLine("Path where BotToken.txt will be stored, this is the bot token given by discord.");
            Console.WriteLine(@"Example: C:\DuckBot\BotToken.txt");
            paths.Add(Console.ReadLine());

            Console.WriteLine();
            Console.WriteLine("Path where ProhibitedWords.txt will be stored, this defines all words or phrases that will trigger a warning from the bot");
            Console.WriteLine(@"Example: C:\DuckBot\ProhibitedWords.txt");
            paths.Add(Console.ReadLine());

            Console.WriteLine("Path where BotOutputLog.txt will be stored, this is a log of all messages received by the bot");
            Console.WriteLine(@"Example: C:\DuckBot\BotOutputLog.txt");
            paths.Add(Console.ReadLine());

            //UserBlacklist.txt
            Console.WriteLine();
            Console.WriteLine("Path where UserWhitelist.txt will be stored, this will store users with elevated priviledges from the bot.");
            Console.WriteLine(@"Example: C:\DuckBot\UserWhitelist.txt");
            paths.Add(Console.ReadLine());

            Console.WriteLine();
            Console.WriteLine("Path where UserBlacklist.txt will be stored, this will store users blocked from the bot.");
            Console.WriteLine(@"Example: C:\DuckBot\UserBlacklist.txt");
            paths.Add(Console.ReadLine());

            Console.WriteLine();
            Console.WriteLine("Path where MarketStocksValue.xml will be stored, this will store the stock cost retrieved from online.");
            Console.WriteLine(@"Example: C:\DuckBot\MarketStocksValue.xml");
            paths.Add(Console.ReadLine());

            Console.WriteLine();
            Console.WriteLine("Path where CommandHelpDescription.xml will be stored, this will store definitions for the help command");
            Console.WriteLine(@"Example: C:\DuckBot\CommandHelpDescription.xml");
            paths.Add(Console.ReadLine());

            Console.WriteLine();
            Console.WriteLine("Path where User info will be stored, MUST BE A FOLDER.");
            Console.WriteLine(@"Example: C:\DuckBot\UserInfo");
            paths.Add(Console.ReadLine());

            Console.WriteLine();
            Console.WriteLine("Path where User stocks will be stored, MUST BE A FOLDER.");
            Console.WriteLine(@"Example: C:\DuckBot\UserStocks");
            paths.Add(Console.ReadLine());

            Console.WriteLine();
            Console.WriteLine("Path where CS:GO skin data will be stored");
            Console.WriteLine(@"Example: C:\DuckBot\skinData.json");
            paths.Add(Console.ReadLine());

            try
            {
                File.WriteAllText(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\config.txt", "");
                CoreMethod.WriteListToFile(paths, true, Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\config.txt");

                Console.WriteLine();
                Console.WriteLine("Setup is complete");
                Console.WriteLine();
                Console.WriteLine("Press any key to continue...");
                Console.ReadLine();

                Console.Clear();
            }
            catch (Exception ex)
            {
                Console.WriteLine();
                Console.WriteLine("An error occurred during setup");
                Console.WriteLine();
                Console.WriteLine("Press any key to continue...");
                Console.WriteLine();
                Console.WriteLine(ex.StackTrace);
                Console.ReadLine();

                Console.Clear();
            }
        }
    }
}
