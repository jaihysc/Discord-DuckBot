using Discord;
using Discord.Commands;
using Discord.WebSocket;
using DuckBot.Core;
using DuckBot.Modules.UserActions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DuckBot.Modules.Moderation
{
    public class ModerationManager
    {
        public static async Task ModerationManagerMessageReceivedAsync(SocketMessage message)
        {
            await MessageReplies(message);
        }

        private static async Task MessageReplies(SocketMessage message)
        {
            //Message detection
            CultureInfo culture = new CultureInfo("en-CA", false);

            await ProhibitedWordsChecker.ProhibitedWordsHandler(message);

            if (culture.CompareInfo.IndexOf(message.Content, "rule34", CompareOptions.IgnoreCase) >= 0 && message.Author.IsBot != true)
            {
                await message.Channel.SendMessageAsync("Woah hey hey hey, watch it! You have to be 18+ to use that command and I guarantee you that you aren't.");
            }

            if (culture.CompareInfo.IndexOf(message.Content, "->fish", CompareOptions.IgnoreCase) >= 0 && message.Author.IsBot != true)
            {
                await message.Channel.SendMessageAsync("Hey! How dare you fish in my pond, no regard for our species and our survival");
            }

        }
    }
}
