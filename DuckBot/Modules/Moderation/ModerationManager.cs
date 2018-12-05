using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DuckBot.Modules.Moderation
{
    public class ModerationManager
    {
        public static async Task ModerationManagerMessageReceived(SocketMessage message)
        {
            await DeleteNonCommandsInCommandsChannel(message);
        }


        private static async Task DeleteNonCommandsInCommandsChannel(SocketMessage message)
        {
            if (message.Channel.Id == 504371769738526752 && !message.ToString().StartsWith(MainProgram.botCommandPrefix))
            {
                var sentMessage = await message.Channel.GetMessagesAsync(1).Flatten();
                await message.Channel.DeleteMessagesAsync(sentMessage);
            }
        }
    }
}
