using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DuckBot.Models
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
}
