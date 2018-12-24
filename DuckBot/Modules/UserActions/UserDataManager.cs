using Discord.Commands;
using Discord.WebSocket;
using DuckBot.Core;
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
    public class UserXmlDataStorage
    {
        public static void CreateNewUserXmlEntry(SocketCommandContext Context)
        {
            var userRecord = new UserStorage
            {
                UserId = Context.Message.Author.Id,
                UserInfo = new UserInfo
                {
                    UserDailyLastUseStorage = new UserDailyLastUseStorage { DateTime = DateTime.UtcNow.AddYears(-1) },
                    UserBankingStorage = new UserBankingStorage { Credit = long.Parse(SettingsManager.RetrieveFromConfigFile("startAmount")), CreditDebt = 0 },
                    UserProhibitedWordsStorage = new UserProhibitedWordsStorage { SwearCount = 0 }
                }
            };
            //var a = XmlManager.FromXmlFile<UserStorage>(CoreMethod.GetFileLocation(@"\UserStorage") + @"\" + Context.Message.Author.Id + ".xml");

            XmlManager.ToXmlFile(userRecord, CoreMethod.GetFileLocation(@"\UserStorage") + @"\" + Context.Message.Author.Id + ".xml");

        }

        public static void CreateNewUserXmlEntry(SocketGuildUser user)
        {
            var userRecord = new UserStorage
            {
                UserId = user.Id,
                UserInfo = new UserInfo
                {
                    UserDailyLastUseStorage = new UserDailyLastUseStorage { DateTime = DateTime.UtcNow.AddYears(-1) },
                    UserBankingStorage = new UserBankingStorage { Credit = long.Parse(SettingsManager.RetrieveFromConfigFile("startAmount")), CreditDebt = 0 },
                    UserProhibitedWordsStorage = new UserProhibitedWordsStorage { SwearCount = 0 }
                }
            };

            XmlManager.ToXmlFile(userRecord, CoreMethod.GetFileLocation(@"\UserStorage") + @"\" + user.Id + ".xml");

        }
    }
}
