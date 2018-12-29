using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using DuckBot.Models;
using DuckBot.Modules.Commands.Preconditions;
using DuckBot.Modules.Finance;
using DuckBot_ClassLibrary;
using DuckBot_ClassLibrary.Modules;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace DuckBot.Modules.Moderation
{
    [UserStorageCheckerPrecondition]
    public class ProhibitedWordsChecker : ModuleBase<SocketCommandContext>
    {
        private static Dictionary<ulong, ProhibitedWordsUserTracker> UserTracker = new Dictionary<ulong, ProhibitedWordsUserTracker>();

        public static async Task ProhibitedWordsHandler(SocketMessage message)
        {
            //Return is sender is a bot
            if (message.Author.IsBot) return;

            CultureInfo culture = new CultureInfo("en-CA", false);
            List<string> blockedWords = new List<string>();

            bool userWhiteListed = GetIsUserWhitelisted(message);
            bool sendSwearWarning = false;

            //Prohibited word detection

            //Reads list of prohibited words from file, checks if message contains words
            var prohibitedWords = File.ReadAllLines(CoreMethod.GetFileLocation("ProhibitedWords.txt"));

            foreach (var forbiddenWord in prohibitedWords)
            {
                if (culture.CompareInfo.IndexOf(message.Content, forbiddenWord, CompareOptions.IgnoreCase) >= 0 && message.Author.IsBot != true)
                {
                    blockedWords.Add(forbiddenWord);
                    sendSwearWarning = true;
                }
            }

            //Sends swear warning to user if previous statement detected swear word
            if (sendSwearWarning == true && userWhiteListed == false)
            {
                string userReturnString = string.Join(", ", blockedWords);

                bool sendWarning = false;
                if (!UserTracker.TryGetValue(message.Author.Id, out var selectedUserTracker))
                {
                    //If user does not exist in tracker, add it
                    UserTracker.Add(message.Author.Id, new ProhibitedWordsUserTracker
                    {
                        SentProhibitedWords = blockedWords,
                        SentTime = DateTime.UtcNow
                    });

                    sendWarning = true;
                }
                //If user has not sent the same words
                else
                {
                    foreach (var badWord in blockedWords)
                    {
                        if (!selectedUserTracker.SentProhibitedWords.Contains(badWord))
                        {
                            sendWarning = true;

                            //Set blocked words to current words
                            UserTracker[message.Author.Id].SentProhibitedWords = blockedWords;
                        }
                    }
                }
                
                //Send swear warning
                if (sendWarning == true)
                {
                    await message.Channel.SendMessageAsync(message.Author.Mention + " HEY `" + message.Author.Username + "` WATCH IT! THIS IS A FUCKING CHRISTIAN FUCKING DISCORD SERVER, `" + userReturnString + "` IS NOT ALLOWED HERE");
                }



                //Logs user swear amount to local counter
                //Get user storage
                var userStorage = XmlManager.FromXmlFile<UserStorage>(CoreMethod.GetFileLocation(@"\UserStorage") + @"\" + message.Author.Id + ".xml");

                userStorage.UserInfo[message.Author.Id].UserProhibitedWordsStorage.SwearCount = userStorage.UserInfo[message.Author.Id].UserProhibitedWordsStorage.SwearCount + 1;

                //write new swear count to user profile
                var userRecord = new UserStorage
                {
                    UserInfo = userStorage.UserInfo
                };

                XmlManager.ToXmlFile(userRecord, CoreMethod.GetFileLocation(@"\UserStorage") + @"\" + message.Author.Id + ".xml");
            }
        }

        private static bool GetIsUserWhitelisted(SocketMessage message)
        {
            // If this command was NOT executed by predefined users, return a failure
            List<ulong> whitelistedUsers = new List<ulong>();

            CoreMethod.ReadFromFileToList("UserWhitelist.txt").ForEach(u => whitelistedUsers.Add(ulong.Parse(u)));

            //Test if user is whitelisted
            bool userIsWhiteListed = false;
            foreach (var user in whitelistedUsers)
            {
                if (message.Author.Id == user)
                {
                    userIsWhiteListed = true;
                }
            }

            bool userWhiteListed = false;
            if (userIsWhiteListed == true)
            {
                userWhiteListed = true;
            }

            return userWhiteListed;
        }
    }
}
