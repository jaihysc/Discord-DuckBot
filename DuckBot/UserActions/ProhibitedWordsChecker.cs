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

namespace DuckBot.UserActions
{
    class ProhibitedWordsChecker : ModuleBase<SocketCommandContext>
    {
        internal static string lastProhibitedWordString;
        internal static string lastProhibitedWordAuthor;

        public static async Task ProhibitedWordsHandler(SocketMessage message, string rootLocation)
        {
            CultureInfo culture = new CultureInfo("en-CA", false);

            bool sendSwearWarning = false;
            List<string> blockedWords = new List<string>();

            //Prohibited word detection
            try
            {
                var configLocations = File.ReadAllLines(rootLocation + @"\Paths.txt");
                var prohibitedWordsFilterLocation = configLocations.Where(p => p.Contains("ProhibitedWords.txt")).ToArray();
                var userProhibitedWordsCounterLocation = configLocations.Where(p => p.Contains("UserProhibitedWordsCounter.txt")).ToArray();

                //Reads list of prohibited words from file, checks if message contains words
                if (message.Author.IsBot == false)
                {
                    foreach (var item in prohibitedWordsFilterLocation)
                    {
                        var prohibitedWords = File.ReadAllLines(item);

                        foreach (var forbiddenWord in prohibitedWords)
                        {
                            if (culture.CompareInfo.IndexOf(message.Content, forbiddenWord, CompareOptions.IgnoreCase) >= 0 && message.Author.IsBot != true)
                            {
                                blockedWords.Add(forbiddenWord);
                                sendSwearWarning = true;
                            }
                        }
                    }

                    //Sends swear warning to user if previous statement detected swear word
                    if (sendSwearWarning == true)
                    {
                        string userReturnString = string.Join(", ", blockedWords);

                        //Send swear warning
                        if (lastProhibitedWordString != userReturnString || lastProhibitedWordAuthor != message.Author.Id.ToString())
                        {
                            await message.Channel.SendMessageAsync(message.Author.Mention + " HEY `" + message.Author.Username + "` WATCH IT! THIS IS A FUCKING CHRISTIAN FUCKING DISCORD SERVER, `" + userReturnString + "` IS NOT ALLOWED HERE");
                        }

                        //Prevent CTRL C+V swear spam || Bot will not reply if last swear word is the same from same author
                        lastProhibitedWordString = message.Content.ToString();
                        lastProhibitedWordAuthor = message.Author.Id.ToString();


                        //Logs user swear amount to local counter
                        foreach (var location in userProhibitedWordsCounterLocation)
                        {
                            bool userExists = false;
                            var prohibitedWordUsers = File.ReadAllLines(location);

                            foreach (var user in prohibitedWordUsers)
                            {
                                try
                                {
                                    if (message.Author.Id.ToString() == user.Substring(0, message.Author.ToString().Length))
                                    {
                                        userExists = true;

                                        string userCounter = user.Substring(message.Author.ToString().Length + 5, user.Length - message.Author.ToString().Length - 5);
                                        string userCounterNew = (int.Parse(userCounter) + 1).ToString();

                                        var newProhibitedWordUsers = prohibitedWordUsers.Where(p => !p.Contains(message.Author.ToString()));
                                        var sortedNewProhibitedWordUsers = newProhibitedWordUsers.OrderBy(x => x).ToList();

                                        File.WriteAllText(location, "");
                                        foreach (var name in sortedNewProhibitedWordUsers)
                                        {
                                            using (System.IO.StreamWriter file =
                                            new System.IO.StreamWriter(location, true))
                                            {
                                                file.WriteLine(name);
                                            }
                                        }

                                        using (System.IO.StreamWriter file =
                                        new System.IO.StreamWriter(location, true))
                                        {
                                            file.WriteLine($"{message.Author.Id.ToString()} >>> {userCounterNew}");
                                        }
                                    }
                                }
                                catch (Exception)
                                {
                                }
                            }

                            //Create txt entry if user is not found
                            if (userExists == false)
                            {
                                File.WriteAllText(location, "");
                                foreach (var name in prohibitedWordUsers)
                                {
                                    using (System.IO.StreamWriter file =
                                    new System.IO.StreamWriter(location, true))
                                    {
                                        file.WriteLine(name);
                                    }
                                }

                                using (System.IO.StreamWriter file =
                                new System.IO.StreamWriter(location, true))
                                {
                                    file.WriteLine($"{message.Author.Id.ToString()} >>> 1");
                                }

                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                Console.WriteLine("Unable to check for prohibited words > ");
            }
        }
    }
}
