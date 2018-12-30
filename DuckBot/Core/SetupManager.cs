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
        /// <summary>
        /// Checks to see if a Paths.txt is present at the execution folder, otherwise it will prompt the user to create one
        /// </summary>
        public static void CheckIfPathsFileExists()
        {
            if (!File.Exists(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\Paths.txt"))
            {
                GenerateConfigFile();
            }
        }

        private static void GenerateConfigFile()
        {
            //Get user input and generate config file
            Console.WriteLine("It appears this is your first startup of DuckBot");
            Console.WriteLine("Configure the bot by entering the paths file where all data will be stored");
            Console.WriteLine();
            string path = Console.ReadLine();

            try
            {
                CoreMethod.WriteStringToFile(path, true, Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\Paths.txt");

                Console.WriteLine();
                Console.WriteLine("Setup is complete");
                Console.WriteLine();
                Console.WriteLine("Press ENTER to continue...");
                Console.ReadLine();

                Console.Clear();
            }
            catch (Exception ex)
            {
                Console.WriteLine();
                Console.WriteLine("An error occurred during setup");
                Console.WriteLine();
                Console.WriteLine("Press ENTER to continue...");
                Console.WriteLine();
                Console.WriteLine(ex.StackTrace);
                Console.ReadLine();

                Console.Clear();
            }
        }
    }
}
