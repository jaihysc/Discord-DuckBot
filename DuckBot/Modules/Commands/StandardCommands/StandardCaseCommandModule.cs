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
    [Group("cs")]
    [Alias("c")]
    public class StandardCaseCommandModule : InteractiveBase<SocketCommandContext>
    {
        [Command("open", RunMode = RunMode.Async)]
        [Alias("o")]
        public async Task OpenCaseAsync()
        {
            await CsgoUnboxingHandler.OpenCase(Context);
        }

        [Command("inventory", RunMode = RunMode.Async)]
        [Alias("i")]
        public async Task DisplayInventoryAsync()
        {
            //Get paginated message
            var pager = CsgoInventoryHandler.DisplayUserCsgoInventory(Context);

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

        [Command("sell", RunMode = RunMode.Async)]
        [Alias("s")]
        public async Task SellInventoryItemAsync([Remainder]string inventoryMarketHash)
        {
            if (inventoryMarketHash == "*")
            {
                await CsgoInventoryTransactionHandler.SellAllInventoryItemAsync(Context);
            }
            else
            {
                await CsgoInventoryTransactionHandler.SellInventoryItemAsync(Context, inventoryMarketHash);
            }           
        }

        [Command("buy", RunMode = RunMode.Async)]
        [Alias("b")]
        public async Task BuyInventoryItemAsync([Remainder]string inventoryMarketHash)
        {
            await CsgoInventoryTransactionHandler.BuyItemFromMarketAsync(Context, inventoryMarketHash);
        }

        [Ratelimit(1, 2, Measure.Minutes)]
        [Command("market", RunMode = RunMode.Async)]
        [Alias("m")]
        public async Task ShowItemMarketAsync([Remainder]string filterString = null)
        {
            var pager = CsGoMarketInventoryHandler.GetCsgoMarketInventory(Context, filterString);

            //Send paginated message
            await PagedReplyAsync(pager, new ReactionList
            {
                Jump = true,
                Forward = true,
                Backward = true,
                Last = true,
                Trash = true,
                First = true
            });
        }
    }
}
