using Discord;
using Discord.Commands;
using DuckBot.Modules.Finance.CurrencyManager;
using DuckBot_ClassLibrary;
using DuckBot_ClassLibrary.Modules;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DuckBot.Modules.CsgoCaseUnboxing
{
    public class CsgoUnboxingHandler
    {
        public static RootWeaponSkin rootWeaponSkin;

        /// <summary>
        /// Opens a virtual CS:GO case, result is sent to Context channel in a method
        /// </summary>
        /// <param name="Context">Command context used to determine channel to send result</param>
        /// <returns></returns>
        public static async Task OpenCase(SocketCommandContext Context)
        {
            //Gets weapon skin data for case unbox
            GetRootWeaponSkin();

            //Test if user has enough credits
            if (UserCreditsHandler.AddCredits(Context, -300) == true)
            {
                var itemProcess = new ItemDropProcessing();
                var result = itemProcess.CalculateItemRarity();

                //While item is not found, try to get a item
                SkinItem skinItem = null;
                while (skinItem == null)
                {
                    //Get item
                    skinItem = itemProcess.GetItem(result, rootWeaponSkin);

                    //Read skin prices from file
                    var rootWeaponSkinPrice = CsgoItemPriceHandler.GetRootWeaponSkin();

                    //Check if a price exists for the item about to be given to the user
                    bool itemPriceExists = false;
                    foreach (var item in rootWeaponSkinPrice.items)
                    {
                        if (item.name == skinItem.market_hash_name)
                        {
                            itemPriceExists = true;
                        }               
                    }

                    //If doesn't exist, try to get another item
                    if (itemPriceExists == false)
                    {
                        skinItem = null;
                    }
                }             

                //Add money for skin quality
                long skinMarketValue = CsgoItemPriceHandler.GetWeaponSkinPrice(skinItem.market_name);
                //UserCreditsHandler.AddCredits(Context, skinMarketValue);

                //Add item to user file inventory
                var userSkin = XmlManager.FromXmlFile<UserSkinStorageRootobject>(CoreMethod.GetFileLocation("UserSkinStorage.xml"));

                userSkin.UserSkinEntries.Add(new UserSkinEntry { Market_hash_name = skinItem.market_hash_name, OwnerID = Context.Message.Author.Id, UnboxDate = DateTime.UtcNow});

                XmlManager.ToXmlFile(userSkin, CoreMethod.GetFileLocation("UserSkinStorage.xml"));

                //Send item into
                var unboxHandler = new CsgoUnboxingHandler();
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
                        .WithText("Sent by " + Context.Message.Author.ToString())
                        .WithIconUrl(Context.Message.Author.GetAvatarUrl());
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

        public static RootWeaponSkin GetRootWeaponSkin()
        {
            if (rootWeaponSkin == null)
            {
                //Read skin data from local json file
                using (StreamReader r = new StreamReader(CoreMethod.GetFileLocation("skinData.json")))
                {
                    string json = r.ReadToEnd();
                    rootWeaponSkin = JsonConvert.DeserializeObject<RootWeaponSkin>(json);
                }
            }

            return rootWeaponSkin;
        }
    }

    internal class ItemDropProcessing
    {
        static Random rand = new Random();
        public ItemRarity CalculateItemRarity()
        {
            int randomNumber = rand.Next(9999);

            //if (randomNumber < 10000 && randomNumber >= 6004) return ItemRarity.LightBlue;
            if (randomNumber < 10000 && randomNumber >= 2008) return ItemRarity.DarkerBlue;
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
