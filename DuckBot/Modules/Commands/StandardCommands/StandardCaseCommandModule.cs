using Discord.Addons.Interactive;
using Discord.Commands;
using DuckBot.Modules.Commands.Preconditions;
using DuckBot.Modules.CsgoCaseUnboxing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DuckBot.Modules.Commands.StandardCommands
{
    [Ratelimit(1, 5, Measure.Seconds)]
    [BlacklistedUsersPrecondition]
    [UserStorageCheckerPrecondition]
    [Group("case")]
    [Alias("c")]
    public class StandardCaseCommandModule : InteractiveBase<SocketCommandContext>
    {
        [Command("open")]
        [Alias("o")]
        public async Task OpenCaseAsync()
        {
            await CsgoUnboxingHandler.OpenCase(Context);
        }

        [Command("inventory")]
        [Alias("i")]
        public async Task DisplayInventoryAsync()
        {
            var csgoInventoryManager = new CsgoInventoryHandler();

            //These 2 lines work by magic - J c
            var pager = csgoInventoryManager.DisplayUserCsgoInventory(Context);

            //Send paginated message
            await PagedReplyAsync(pager, new ReactionList
            {
                //Jump = true,
                Forward = true,
                Backward = true,
                Last = true,
                Trash = true,
                First = true
            });
        }

        [Command("sell")]
        [Alias("s")]
        public async Task SellInventoryItemAsync([Remainder]string inventoryMarketHash)
        {
            if (inventoryMarketHash == "*")
            {
                CsgoInventorySaleHandler.SellAllInventoryItem(Context);
            }
            else
            {
                CsgoInventorySaleHandler.SellInventoryItem(Context, inventoryMarketHash);
            }           
        }
    }
}
