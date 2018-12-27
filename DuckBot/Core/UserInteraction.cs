using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DuckBot.Core
{
    public class UserInteraction
    {
        public static string BoldUserName(SocketCommandContext context)
        {
            return $"**{context.Message.Author.ToString().Substring(0, context.Message.Author.ToString().Length - 5)}**";
        }
    }
}
