using Discord.Commands;
using DuckBot.UserActions;
using DuckBot_ClassLibrary;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DuckBot.Finance.CurrencyManager
{
    public class UserBankingHandler
    {
        public static void CheckIfUserCreditProfileExists(SocketCommandContext Context)
        {
            //Create txt user credit entry if user does not exist
            if (!File.Exists(CoreMethod.GetFileLocation(@"\UserStorage") + @"\" + Context.Message.Author.Id + ".xml"))
            {
                //Create user profile
                UserXmlDataStorage.CreateNewUserXmlEntry(Context);
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
