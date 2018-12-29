using Discord.Commands;
using Discord.WebSocket;
using DuckBot.Modules.UserActions;
using DuckBot_ClassLibrary;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DuckBot.Modules.Finance.CurrencyManager
{
    public class UserBankingHandler
    {
        /// <summary>
        /// This checks if a user profile exists, if not, it will create a profile
        /// </summary>
        public static void CheckIfUserCreditProfileExists(SocketCommandContext context)
        {
            var userStorage = UserDataManager.GetUserStorage();
            //Create txt user credit entry if user does not exist
            if (!userStorage.UserInfo.TryGetValue(context.Message.Author.Id, out var i))
            {
                //Create user profile
                UserDataManager.CreateNewUserXmlEntry(context);
            }
        }

        public static void CheckIfUserCreditProfileExists(SocketGuildUser user)
        {
            var userStorage = UserDataManager.GetUserStorage();
            //Create txt user credit entry if user does not exist
            if (!userStorage.UserInfo.TryGetValue(user.Id, out var i))
            {
                //Create user profile
                UserDataManager.CreateNewUserXmlEntry(user);
            }
        }

        public static string CreditCurrencyFormatter(long inputCredits)
        {
            //Formats number to use currency numeration
            var numberGroupSeperator = new NumberFormatInfo { NumberGroupSeparator = " " };

            return inputCredits.ToString("N0", numberGroupSeperator);
        }
    }
}
