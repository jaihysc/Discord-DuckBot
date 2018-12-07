using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
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

namespace DuckBot.Modules.UserActions
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
                        if (lastProhibitedWordString != message.ToString() || lastProhibitedWordAuthor != message.Author.Id.ToString())
                        {
                            await message.Channel.SendMessageAsync(message.Author.Mention + " HEY `" + message.Author.Username + "` WATCH IT! THIS IS A FUCKING CHRISTIAN FUCKING DISCORD SERVER, `" + userReturnString + "` IS NOT ALLOWED HERE");
                        }

                        //Prevent CTRL C+V swear spam || Bot will not reply if last swear word is the same from same author
                        lastProhibitedWordString = message.Content.ToString();
                        lastProhibitedWordAuthor = message.Author.Id.ToString();


                        //Logs user swear amount to local counter
                        //Create txt user credit entry if user does not exist
                        if (!File.Exists(CoreMethod.GetFileLocation(@"\UserStorage") + @"\" + message.Author.Id + ".xml"))
                        {
                            //Create user profile
                            var userRecord = new UserStorage
                            {
                                UserId = message.Author.Id,
                                UserInfo = new UserInfo
                                {
                                    UserDailyLastUseStorage = new UserDailyLastUseStorage { DateTime = DateTime.UtcNow.AddYears(-1) },
                                    UserBankingStorage = new UserBankingStorage { Credit = 10000, CreditDebt = 0 },
                                    UserProhibitedWordsStorage = new UserProhibitedWordsStorage { SwearCount = 0 }
                                }
                            };

                            XmlManager.ToXmlFile(userRecord, CoreMethod.GetFileLocation(@"\UserStorage") + @"\" + message.Author.Id + ".xml");
                        }

                        //Get user storage
                        var userStorage = XmlManager.FromXmlFile<UserStorage>(CoreMethod.GetFileLocation(@"\UserStorage") + @"\" + message.Author.Id + ".xml");

                        //write new swear count to user profile
                        var userRecordNew = new UserStorage
                        {
                            UserId = userStorage.UserId,
                            UserInfo = new UserInfo
                            {
                                UserDailyLastUseStorage = new UserDailyLastUseStorage { DateTime = userStorage.UserInfo.UserDailyLastUseStorage.DateTime },
                                UserBankingStorage = new UserBankingStorage { Credit = userStorage.UserInfo.UserBankingStorage.Credit, CreditDebt = userStorage.UserInfo.UserBankingStorage.CreditDebt },
                                UserProhibitedWordsStorage = new UserProhibitedWordsStorage { SwearCount = userStorage.UserInfo.UserProhibitedWordsStorage.SwearCount + 1 }
                            }
                        };

                        XmlManager.ToXmlFile(userRecordNew, CoreMethod.GetFileLocation(@"\UserStorage") + @"\" + message.Author.Id + ".xml");
                    }
                }
            }
            catch (Exception)
            {
            }
        }
    }
}
