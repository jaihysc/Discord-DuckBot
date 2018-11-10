using DuckBot.UserActions;
using DuckBot_ClassLibrary;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DuckBot.Finance.ServiceThreads
{
    public class UserBankingInterestUpdater
    {
        //Interest percentage expressed as a fraction
        internal static double interestPercentage = 0.60;

        ///<Summary>
        ///Updates the user banking debt every set amount of milliseconds, wait duration hardcoded
        ///</Summary>
        public static void UpdateUserDebtInterest()
        {
            while (MainProgram._stopThreads == false)
            {
                try
                {
                    //Log action
                    Console.WriteLine("User debt updated - " + DateTime.Now);

                    UserDebtInterestUpdater();
                }
                catch (Exception)
                {
                }

                //Sleep for 30 minutes
                Thread.Sleep(180000);
            }
        }

        ///<Summary>
        ///Increases user borrowed debt by set percentage
        ///</Summary>
        public static void UserDebtInterestUpdater()
        {
            //Update user debt
            foreach (string file in Directory.EnumerateFiles(TaskMethods.GetFileLocation(@"\UserStorage"), "*.xml"))
            {
                var userCreditStorage = XmlManager.FromXmlFile<UserStorage>(file);

                //Calculate new debt with interest
                int debtAmountNew = Convert.ToInt32((userCreditStorage.UserInfo.UserBankingStorage.CreditDebt * interestPercentage) + userCreditStorage.UserInfo.UserBankingStorage.CreditDebt);

                //Write to file
                var userRecord = new UserStorage
                {
                    UserId = userCreditStorage.UserId,
                    UserInfo = new UserInfo
                    {
                        UserDailyLastUseStorage = new UserDailyLastUseStorage { DateTime = userCreditStorage.UserInfo.UserDailyLastUseStorage.DateTime },
                        UserBankingStorage = new UserBankingStorage { Credit = userCreditStorage.UserInfo.UserBankingStorage.Credit, CreditDebt = Convert.ToInt32(debtAmountNew) },
                        UserProhibitedWordsStorage = new UserProhibitedWordsStorage { SwearCount = userCreditStorage.UserInfo.UserProhibitedWordsStorage.SwearCount }
                    }
                };

                XmlManager.ToXmlFile(userRecord, file);
            }
        }
    }
}
