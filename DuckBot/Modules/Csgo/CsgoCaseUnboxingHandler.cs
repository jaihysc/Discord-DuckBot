using Discord;
using Discord.Commands;
using DuckBot.Models;
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

namespace DuckBot.Modules.Csgo
{
    public class CsgoUnboxingHandler
    {
        private static RootSkinData rootWeaponSkin;

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


                //Get item
                var skinItem = itemProcess.GetItem(result, rootWeaponSkin);


                //Add item to user file inventory
                var userSkin = XmlManager.FromXmlFile<UserSkinStorageRootobject>(CoreMethod.GetFileLocation("UserSkinStorage.xml"));

                userSkin.UserSkinEntries.Add(new UserSkinEntry { ClassId = skinItem.Classid, OwnerID = Context.Message.Author.Id, UnboxDate = DateTime.UtcNow, MarketName = skinItem.Name});

                XmlManager.ToXmlFile(userSkin, CoreMethod.GetFileLocation("UserSkinStorage.xml"));

                //Send item into
                await SendOpenedCaseInfo(Context, skinItem, Convert.ToInt64(skinItem.Price.AllTime.Average));
            }
            else
            {
                await Context.Channel.SendMessageAsync("**" + Context.Message.Author.ToString().Substring(0, Context.Message.Author.ToString().Length - 5) + ", **You do not have enough credits to unbox a case");
            }
        }

        public static async Task SendOpenedCaseInfo(SocketCommandContext Context, SkinDataItem skinItem, long skinMarketValue)
        {
            var embedBuilder = new EmbedBuilder()
                .WithColor(new Color(Convert.ToUInt32(skinItem.RarityColor, 16)))
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
                .AddField(skinItem.Name, $"Market Value: {skinMarketValue}")
                .WithImageUrl("https://steamcommunity.com/economy/image/" + skinItem.IconUrlLarge);

            var embed = embedBuilder.Build();

            await Context.Message.Channel.SendMessageAsync(" ", embed: embed).ConfigureAwait(false);
        }

        public static RootSkinData GetRootWeaponSkin()
        {
            if (rootWeaponSkin == null)
            {
                //Read skin data from local json file
                using (StreamReader r = new StreamReader(CoreMethod.GetFileLocation("skinData.json")))
                {
                    string json = r.ReadToEnd();
                    var rootWeaponSkin = RootSkinData.FromJson(json);

                    var rootWeaponSkinTemp = rootWeaponSkin;

                    foreach (var item in rootWeaponSkin.ItemsList.Values)
                    {
                        try
                        {
                            //Multiply all prices by 100 to remove decimals
                            if (item.Price != null)
                            {
                                rootWeaponSkinTemp.ItemsList[item.Name].Price.AllTime.Average = item.Price.AllTime.Average * 100;
                            }
                            else
                            {
                                rootWeaponSkinTemp.ItemsList = rootWeaponSkinTemp.ItemsList.Where(s => s.Key != item.Name).ToDictionary(x => x.Key, y => y.Value);
                            }
                        }
                        catch (Exception)
                        {
                        }            

                        /*
                        item.Value.Price.AllTime.HighestPrice = item.Value.Price.AllTime.HighestPrice * 100;
                        item.Value.Price.AllTime.LowestPrice = item.Value.Price.AllTime.LowestPrice * 100;
                        item.Value.Price.AllTime.Median = item.Value.Price.AllTime.Median * 100;

                        
                        item.Value.Price.The24_Hours.Average = item.Value.Price.The24_Hours.Average * 100;
                        item.Value.Price.The24_Hours.HighestPrice = item.Value.Price.The24_Hours.HighestPrice * 100;
                        item.Value.Price.The24_Hours.LowestPrice = item.Value.Price.The24_Hours.LowestPrice * 100;
                        item.Value.Price.The24_Hours.Median = item.Value.Price.The24_Hours.Median * 100;

                        item.Value.Price.The30_Days.Average = item.Value.Price.The30_Days.Average * 100;
                        item.Value.Price.The30_Days.HighestPrice = item.Value.Price.The30_Days.HighestPrice * 100;
                        item.Value.Price.The30_Days.LowestPrice = item.Value.Price.The30_Days.LowestPrice * 100;
                        item.Value.Price.The30_Days.Median = item.Value.Price.The30_Days.Median * 100;

                        item.Value.Price.The7_Days.Average = item.Value.Price.The7_Days.Average * 100;
                        item.Value.Price.The7_Days.HighestPrice = item.Value.Price.The7_Days.HighestPrice * 100;
                        item.Value.Price.The7_Days.LowestPrice = item.Value.Price.The7_Days.LowestPrice * 100;
                        item.Value.Price.The7_Days.Median = item.Value.Price.The7_Days.Median * 100;
                        */

                    }


                    rootWeaponSkin = rootWeaponSkinTemp;
                    CsgoUnboxingHandler.rootWeaponSkin = rootWeaponSkin;
                }
            }


            return rootWeaponSkin;
        }
    }

    internal class ItemDropProcessing
    {
        static Random rand = new Random();


        public ItemListType CalculateItemRarity()
        {
            int randomNumber = rand.Next(9999);

            //if (randomNumber < 10000 && randomNumber >= 6004) return new ItemListType{Rarity = Rarity.White };
            if (randomNumber < 10000 && randomNumber >= 2008) return new ItemListType { Rarity = Rarity.MilSpecGrade };
            if (randomNumber < 2008 && randomNumber >= 410) return new ItemListType { Rarity = Rarity.Restricted };
            if (randomNumber < 410 && randomNumber >= 90) return new ItemListType { Rarity = Rarity.Classified };
            if (randomNumber < 90 && randomNumber >= 26) return new ItemListType { Rarity = Rarity.Covert, BlackListWeaponType = WeaponType.Knife};
            if (randomNumber < 26 && randomNumber >= 0) return new ItemListType { Rarity = Rarity.Covert, WeaponType = WeaponType.Knife };

            return new ItemListType { Rarity = Rarity.MilSpecGrade };
        }

        /// <summary>
        /// 1 in 10 chance of stat trak, (Returns true for stattrak)
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// Fetches and randomly retrieves a skin item of specified type
        /// </summary>
        /// <param name="itemListType">Type file</param>
        /// <param name="skinData">Skin data to look through</param>
        /// <returns></returns>
        public SkinDataItem GetItem(ItemListType itemListType, RootSkinData skinData)
        {
            var sortedResult = skinData.ItemsList
                .Where(s => s.Value.Rarity == itemListType.Rarity).ToList();

            //If weaponType is not null, filter by weapon type
            if (itemListType.WeaponType != null)
            {
                sortedResult = sortedResult
                .Where(s => s.Value.WeaponType == itemListType.WeaponType).ToList();
            }

            //If blackListWeaponType is not null, filter by weapon type
            if (itemListType.BlackListWeaponType != null)
            {
                sortedResult = sortedResult
                .Where(s => s.Value.WeaponType != itemListType.BlackListWeaponType).ToList();
            }

            //Filter out stattrak, stickers, music kits, and graffiti
            sortedResult = sortedResult
                .Where(s => !s.Value.Name.ToLower().Contains("stattrak"))
                .Where(s => !s.Value.Name.ToLower().Contains("sticker"))
                .Where(s => !s.Value.Name.ToLower().Contains("music kit"))
                .Where(s => !s.Value.Name.ToLower().Contains(" pin"))
                .Where(s => !s.Value.Name.ToLower().Contains("souvenir"))
                .Where(s => !s.Value.Name.ToLower().Contains("graffiti")).ToList();


            //Randomly select a skin from the filtered list of possible skins
            var selectedSkin = sortedResult[rand.Next(sortedResult.Count())];


            //Give stattrak
            if (CalculateStatTrakDrop() == true)
            {
                var selectedStatTrakItem = skinData.ItemsList
                    .Where(s => s.Value.Classid.ToLower().Contains(selectedSkin.Value.Classid))
                    .Where(s => s.Value.Name.ToLower().Contains("stattrak")).FirstOrDefault();


                 selectedSkin = selectedStatTrakItem;
            }

            
            return selectedSkin.Value;
        }
    }
}
