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
        internal static double interestPercentage = 0.20;

        public static void UpdateUserDebtInterest()
        {
            while (MainProgram._stopThreads == false)
            {
                try
                {
                    List<string> userDebtAmountStorage = CoreMethod.ReadFromFileToList("UserCreditsDebt.txt");

                    //Get last update time
                    DateTime interestLastUpdateDate = HelperMethod.GetLastUpdateTime(userDebtAmountStorage);

                    //If 1 hour has passed, update user interest
                    if (interestLastUpdateDate.AddHours(1) < DateTime.UtcNow)
                    {
                        //Log action
                        Console.WriteLine("User debt updated - " + DateTime.Now);

                        //Clear file for new debt amounts
                        File.WriteAllText(CoreMethod.GetFileLocation("UserCreditsDebt.txt"), "");

                        //Update user debt
                        List<string> sortedUserDebtStorage = userDebtAmountStorage.Where(p => !p.Contains(HelperMethod.updateTimeContainer)).ToList();
                        foreach (var userEntry in sortedUserDebtStorage)
                        {
                            //Get user debt
                            int userIdLength = 0;

                            userIdLength = userEntry.IndexOf(" ", StringComparison.Ordinal);
                            string oldDebtAmount = userEntry.Substring(userIdLength + 5, userEntry.Length - userIdLength - 5);

                            //Calculate new debt with interest
                            Int64 debtAmountNew = Convert.ToInt64((int.Parse(oldDebtAmount) * interestPercentage) + int.Parse(oldDebtAmount));

                            if (int.Parse(oldDebtAmount) > 0 && debtAmountNew == int.Parse(oldDebtAmount))
                            {
                                debtAmountNew++;
                            }

                            //Log in console
                            Console.WriteLine(userEntry.Substring(0, userIdLength) + " >>> " + debtAmountNew);

                            //Write new debt
                            using (System.IO.StreamWriter file = new System.IO.StreamWriter(CoreMethod.GetFileLocation("UserCreditsDebt.txt"), true))
                            {
                                file.WriteLine(userEntry.Substring(0, userIdLength) + " >>> " + debtAmountNew);
                            }
                        }

                        //Write new debt last update date
                        using (System.IO.StreamWriter file = new System.IO.StreamWriter(CoreMethod.GetFileLocation("UserCreditsDebt.txt"), true))
                        {
                            file.WriteLine(HelperMethod.updateTimeContainer + " >>> " + DateTime.UtcNow);
                        }

                    }

                    //Sleep for 1 hour
                    Thread.Sleep(3600000);
                }
                catch (Exception)
                {
                }
            }
        }
    }
}
