using Discord.Commands;
using Discord.WebSocket;
using DuckBot.Core;
using DuckBot.Models;
using DuckBot_ClassLibrary;
using DuckBot_ClassLibrary.Modules;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DuckBot.Modules.UserActions
{
    public class UserDataManager
    {
        public static void CreateNewUserXmlEntry(SocketCommandContext context)
        {
            var userStorage = GetUserStorage();

            userStorage.UserInfo.Add(context.Message.Author.Id, new UserInfo
            {
                UserId = context.Message.Author.Id,
                UserDailyLastUseStorage = new UserDailyLastUseStorage { DateTime = DateTime.UtcNow.AddYears(-1) },
                UserBankingStorage = new UserBankingStorage { Credit = long.Parse(SettingsManager.RetrieveFromConfigFile("startAmount")), CreditDebt = 0 },
                UserProhibitedWordsStorage = new UserProhibitedWordsStorage { SwearCount = 0 }
            });

            var userRecord = new UserStorage
            {
                UserInfo = userStorage.UserInfo
            };


            WriteUserStorage(userRecord);
        }

        public static void CreateNewUserXmlEntry(SocketGuildUser user)
        {
            var userStorage = GetUserStorage();

            userStorage.UserInfo.Add(user.Id, new UserInfo
            {
                UserId = user.Id,
                UserDailyLastUseStorage = new UserDailyLastUseStorage { DateTime = DateTime.UtcNow.AddYears(-1) },
                UserBankingStorage = new UserBankingStorage { Credit = long.Parse(SettingsManager.RetrieveFromConfigFile("startAmount")), CreditDebt = 0 },
                UserProhibitedWordsStorage = new UserProhibitedWordsStorage { SwearCount = 0 }
            });

            var userRecord = new UserStorage
            {
                UserInfo = userStorage.UserInfo
            };


            WriteUserStorage(userRecord);
        }

        public static UserStorage GetUserStorage()
        {
            var json = CoreMethod.ReadFromFile(CoreMethod.GetFileLocation("UserStorage.json"));
            return JsonConvert.DeserializeObject<UserStorage>(json);
        }
        public static void WriteUserStorage(UserStorage userStorage)
        {
            string jsonToWrite = JsonConvert.SerializeObject(userStorage);
            CoreMethod.WriteStringToFile(jsonToWrite, true, CoreMethod.GetFileLocation("UserStorage.json"));
        }
    }
}
