using Discord;
using System;

namespace DuckBot.Models
{
    public class CommandTimeout
    {
        public uint TimesInvoked { get; set; }
        public DateTime FirstInvoke { get; }
        public bool ReceivedError { get; set; }

        public CommandTimeout(DateTime timeStarted)
        {
            FirstInvoke = timeStarted;
            ReceivedError = false;
        }
    }
}
