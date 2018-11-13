using Discord.Commands;
using DuckBot_ClassLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DuckBot.UserActions
{
    public class UserStorage
    {
        public ulong UserId { get; set; }
        public UserInfo UserInfo { get; set; }
    }
    public class UserInfo
    {
        public UserDailyLastUseStorage UserDailyLastUseStorage { get; set; }
        public UserBankingStorage UserBankingStorage { get; set; }
        public UserProhibitedWordsStorage UserProhibitedWordsStorage { get; set; }
    }

    public class UserDailyLastUseStorage
    {
        public DateTime DateTime { get; set; }
    }
    public class UserBankingStorage
    {
        public long Credit { get; set; }
        public long CreditDebt { get; set; }
    }
    public class UserProhibitedWordsStorage
    {
        public int SwearCount { get; set; }
    }

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
                    UserBankingStorage = new UserBankingStorage { Credit = 10000, CreditDebt = 0 },
                    UserProhibitedWordsStorage = new UserProhibitedWordsStorage { SwearCount = 0 }
                }
            };
            //var a = XmlManager.FromXmlFile<UserStorage>(TaskMethods.GetFileLocation(@"\UserStorage") + @"\" + Context.Message.Author.Id + ".xml");

            XmlManager.ToXmlFile(userRecord, TaskMethods.GetFileLocation(@"\UserStorage") + @"\" + Context.Message.Author.Id + ".xml");

        }
    }
}
