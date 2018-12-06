﻿using Discord;
using Discord.Commands;
using DuckBot.Modules.Finance.CurrencyManager;
using DuckBot_ClassLibrary;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DuckBot.Modules.CsgoCaseUnboxing
{
    public class UnboxingHandler
    {
        /// <summary>
        /// Opens a virtual CS:GO case, result is sent to Context channel in a method
        /// </summary>
        /// <param name="Context">Command context used to determine channel to send result</param>
        /// <returns></returns>
        public static async Task OpenCase(SocketCommandContext Context)
        {
            //Test if user has enough credits
            if (UserCreditsHandler.AddCredits(Context, -300) == true)
            {
                var itemProcess = new ItemDropProcessing();
                var result = itemProcess.CalculateItemRarity();

                //Get item
                SkinItem skinItem = itemProcess.GetItem(result, GetRootWeaponSkin());

                //Add money for skin quality
                long skinMarketValue = itemProcess.GiveItemQualityCredits(Context, result);

                //Send item into
                var unboxHandler = new UnboxingHandler();
                await unboxHandler.SendOpenedCaseInfo(Context, skinItem, skinMarketValue);
            }
            else
            {
                await Context.Channel.SendMessageAsync("**" +Context.Message.Author.ToString().Substring(0, Context.Message.Author.ToString().Length - 5) + ", **You do not have enough credits to unbox a case");
            }
        }

        private async Task SendOpenedCaseInfo(SocketCommandContext Context, SkinItem skinItem, long skinMarketValue)
        {
            var embedBuilder = new EmbedBuilder()
                .WithColor(new Color(Convert.ToUInt32(skinItem.quality_color, 16)))
                .WithFooter(footer =>
                {
                    footer
                        .WithText("Sent by " + Context.Message.Author.ToString());
                })
                .WithAuthor(author =>
                {
                    author
                        .WithName("Case Unboxing")
                        .WithIconUrl("https://i.redd.it/1s0j5e4fhws01.png");
                })
                .AddField(skinItem.market_name, $"Market Value: {skinMarketValue}")
                .WithImageUrl("http:" + skinItem.icon_url);

            var embed = embedBuilder.Build();

            await Context.Message.Channel.SendMessageAsync(" ", embed: embed).ConfigureAwait(false);
        }

        private static RootWeaponSkin GetRootWeaponSkin()
        {
            //Read skin data from local json file
            RootWeaponSkin rootObject;
            using (StreamReader r = new StreamReader(CoreMethod.GetFileLocation("skinData.json")))
            {
                string json = r.ReadToEnd();
                rootObject = JsonConvert.DeserializeObject<RootWeaponSkin>(json);
            }
            return rootObject;
        }
    }

    internal class ItemDropProcessing
    {
        static Random rand = new Random();
        public ItemRarity CalculateItemRarity()
        {
            int randomNumber = rand.Next(9999);

            if (randomNumber < 10000 && randomNumber >= 6004) return ItemRarity.LightBlue;
            if (randomNumber < 6004 && randomNumber >= 2008) return ItemRarity.DarkerBlue;
            if (randomNumber < 2008 && randomNumber >= 410) return ItemRarity.Purple;
            if (randomNumber < 410 && randomNumber >= 90) return ItemRarity.Pink;
            if (randomNumber < 90 && randomNumber >= 26) return ItemRarity.Red;
            if (randomNumber < 26 && randomNumber >= 0) return ItemRarity.Gold;

            return ItemRarity.LightBlue;
        }

        public bool CalculateStatTrakDrop()
        {
            if (rand.Next(9) == 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public SkinItem GetItem(ItemRarity itemRarity, RootWeaponSkin rootObject)
        {
            string filterQualityColor = "";
            string filterNameColor = "";

            if (itemRarity == ItemRarity.White) { filterQualityColor = "B0C3D9"; filterNameColor = "D2D2D2"; }
            if (itemRarity == ItemRarity.LightBlue) { filterQualityColor = "5E98D9"; filterNameColor = "D2D2D2"; }
            if (itemRarity == ItemRarity.DarkerBlue) { filterQualityColor = "4B69FF"; filterNameColor = "D2D2D2"; }
            if (itemRarity == ItemRarity.Purple) { filterQualityColor = "8847FF"; filterNameColor = "D2D2D2"; }
            if (itemRarity == ItemRarity.Pink) { filterQualityColor = "D32CE6"; filterNameColor = "D2D2D2"; }
            if (itemRarity == ItemRarity.Red) { filterQualityColor = "EB4B4B"; filterNameColor = "D2D2D2"; }
            if (itemRarity == ItemRarity.Gold) { filterQualityColor = "EB4B4B"; filterNameColor = "8650AC"; }
            if (itemRarity == ItemRarity.Yellow) { filterQualityColor = "E4AE39"; filterNameColor = "D2D2D2"; }



            var sortedResult = rootObject.items
                .Where(e => e.quality_color.ToLower().Contains(filterQualityColor.ToLower()))
                .Where(e => e.name_color.ToLower().Contains(filterNameColor.ToLower()))
                .Where(e => !e.market_name.ToLower().Contains("stattrak"))
                .Where(e => !e.market_name.ToLower().Contains("sticker"))
                .Where(e => !e.market_name.ToLower().Contains("graffiti")).ToArray();

            var returnResult = sortedResult[rand.Next(sortedResult.Count())];


            //Give stattrak
            if (CalculateStatTrakDrop() == true)
            {
                var statTrakItems = rootObject.items
                    .Where(e => e.market_name.Contains(returnResult.market_name))
                    .Where(e => e.market_name.ToLower().Contains("stattrak"));

                foreach (var item in statTrakItems)
                {
                    returnResult = item;
                }
            }


            return returnResult;
        }

        public long GiveItemQualityCredits(SocketCommandContext Context, ItemRarity itemRarity)
        {
            //Give credits on item quality
            if (itemRarity == ItemRarity.White) { UserCreditsHandler.AddCredits(Context , 3); return 3; }
            if (itemRarity == ItemRarity.LightBlue) { UserCreditsHandler.AddCredits(Context, 10); return 10; }
            if (itemRarity == ItemRarity.DarkerBlue) { UserCreditsHandler.AddCredits(Context, 50); return 50; }
            if (itemRarity == ItemRarity.Purple) { UserCreditsHandler.AddCredits(Context, 300); return 300; }
            if (itemRarity == ItemRarity.Pink) { UserCreditsHandler.AddCredits(Context, 900); return 900; }
            if (itemRarity == ItemRarity.Red) { UserCreditsHandler.AddCredits(Context, 6000); return 6000; }
            if (itemRarity == ItemRarity.Gold) { UserCreditsHandler.AddCredits(Context, 100000); return 100000; }
            if (itemRarity == ItemRarity.Yellow) { UserCreditsHandler.AddCredits(Context, 500000); return 500000; } 

            return 0;
        }


        public enum ItemRarity { White, LightBlue, DarkerBlue, Purple, Pink, Red, Gold, Yellow }
    }

    public class SkinItem
    {
        public string market_name { get; set; }
        public string market_hash_name { get; set; }
        public string icon_url { get; set; }
        public string name_color { get; set; }
        public string quality_color { get; set; }
    }

    public class RootWeaponSkin
    {
        public bool success { get; set; }
        public int num_items { get; set; }
        public List<SkinItem> items { get; set; }
    }
}