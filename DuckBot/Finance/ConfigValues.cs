﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DuckBot.Finance
{
    public class ConfigValues
    {
        //Banking
        //

        //Percentage between 0-1, expressed as a fraction
        public static double taxPercentage = 0.10;

        public static long dailyAmount = 50000;
        public static long startingCredits = 100000;
   
        public static long maxBorrowAmount = 5000000;
    }
}
