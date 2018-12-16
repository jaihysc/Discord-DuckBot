using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using DuckBot.Core;
using DuckBot.Modules.Finance.CurrencyManager;
using DuckBot_ClassLibrary;
using DuckBot_ClassLibrary.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DuckBot.Modules.CsgoCaseUnboxing
{
    public static class CsgoInventoryHandler
    {
        private static List<string> embedFieldsMaster = new List<string>();
        private static List<string> embedPriceFieldsMaster = new List<string>();

        public static PaginatedMessage DisplayUserCsgoInventory(SocketCommandContext Context)
        {
            //Reset fields
            embedFieldsMaster = new List<string>();
            embedPriceFieldsMaster = new List<string>();

            //Get user skins from xml file
            UserSkinStorageRootobject userSkin = new UserSkinStorageRootobject();
            try
            {
                userSkin = XmlManager.FromXmlFile<UserSkinStorageRootobject>(CoreMethod.GetFileLocation("UserSkinStorage.xml"));
            }
            catch (Exception)
            {
            }

            List<UserSkinEntry> foundUserSkins = new List<UserSkinEntry>();
            //Filter userSkinEntries xml file down to skins belonging to sender
            foreach (var userSkinEntry in userSkin.UserSkinEntries)
            {
                //Filter skin search to those owned by user
                if (userSkinEntry.OwnerID == Context.Message.Author.Id)
                {
                    foundUserSkins.Add(new UserSkinEntry { OwnerID = Context.Message.Author.Id, Market_hash_name = userSkinEntry.Market_hash_name, UnboxDate = userSkinEntry.UnboxDate });
                }
            }

            //Generate fields
            AddSkinFieldEntry(foundUserSkins);

            //Configurate paginated message
            var paginationConfig = new PaginationConfig
            {
                AuthorName = Context.Message.Author.ToString().Substring(0, Context.Message.Author.ToString().Length - 5) + " Inventory",
                AuthorURL = Context.Message.Author.GetAvatarUrl(),

                Description = $"To sell items, use `{MainProgram.botCommandPrefix} case sell [name]`",

                DefaultFieldHeader = "You do not have any skins",
                DefaultFieldDescription = $"Go unbox some with `{MainProgram.botCommandPrefix} case open`",

                Field1Header = "Item Name",
                Field2Header = "Market Value",
            };

            var paginationManager = new PaginationManager();

            //Generate paginated message
            var pager = paginationManager.GeneratePaginatedMessage(embedFieldsMaster, embedPriceFieldsMaster, paginationConfig);

            return pager;
        }

        private static void AddSkinFieldEntry(List<UserSkinEntry> foundUserSkins)
        {
            var rootWeaponSkin = CsgoUnboxingHandler.GetRootWeaponSkin();

            //For every item belonging to sender
            foreach (var item in foundUserSkins)
            {
                //Find skin entry info
                foreach (var storageSkinEntry in rootWeaponSkin.items)
                {
                    //Filter by market hash name
                    //LESSON LEARNED: Decode unicode before processing them to avoid them not being recognised!!!!!!!111!!
                    if (UnicodeLiteralConverter.DecodeToNonAsciiCharacters(storageSkinEntry.market_hash_name) == UnicodeLiteralConverter.DecodeToNonAsciiCharacters(item.Market_hash_name))
                    {
                        string skinQualityEmote = "<:white:522875796319240193>";

                        //Assign quality colors
                        if (storageSkinEntry.quality_color == "B0C3D9" && storageSkinEntry.name_color == "D2D2D2") skinQualityEmote = "<:white:522875796319240193>"; //white
                        if (storageSkinEntry.quality_color == "5E98D9" && storageSkinEntry.name_color == "D2D2D2") skinQualityEmote = "<:lightblue:522878230848602131>"; //light blue
                        if (storageSkinEntry.quality_color == "4B69FF" && storageSkinEntry.name_color == "D2D2D2") skinQualityEmote = "<:darkerblue:522878230550544387>"; //darker blue
                        if (storageSkinEntry.quality_color == "8847FF" && storageSkinEntry.name_color == "D2D2D2") skinQualityEmote = "<:purple:522878233482625034>"; //purple
                        if (storageSkinEntry.quality_color == "D32CE6" && storageSkinEntry.name_color == "D2D2D2") skinQualityEmote = "<:pink:522878230856990807>"; //pink
                        if (storageSkinEntry.quality_color == "EB4B4B" && storageSkinEntry.name_color == "D2D2D2") skinQualityEmote = "<:red:522878230533767199>"; //red
                        if (storageSkinEntry.quality_color == "EB4B4B" && storageSkinEntry.name_color == "8650AC") skinQualityEmote = "<:gold:522878230634692619>"; //gold
                        if (storageSkinEntry.quality_color == "E4AE39" && storageSkinEntry.name_color == "D2D2D2") skinQualityEmote = "<:yellowgold:522878230923968513>"; //rare gold

                        //Add skin entry
                        try
                        {
                            Emote emote = Emote.Parse(skinQualityEmote);

                            //Add skin entry to list
                            embedFieldsMaster.Add(emote + " " + storageSkinEntry.market_name);

                            //Get skin data
                            var rootWeaponSkinPrice = CsgoItemPriceHandler.GetRootWeaponSkin();

                            //Filter and Add skin price entry to list
                            embedPriceFieldsMaster.Add(emote + " " + rootWeaponSkinPrice.items.Where(s => s.name == storageSkinEntry.market_hash_name).FirstOrDefault().price.ToString());
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Source);
                        }
                    }
                }
            }

        }

    }

    public static class CsgoInventoryTransactionHandler
    {
        public static async Task BuyItemFromMarketAsync(SocketCommandContext Context, string itemMarketHash)
        {
            //Get price data
            var rootWeaponSkinPrice = CsgoItemPriceHandler.GetRootWeaponSkin();

            try
            {
                var rootWeaponSkins = CsgoUnboxingHandler.GetRootWeaponSkin();
                UserSkinEntry selectedMarketSkin = new UserSkinEntry();

                //Get market skin cost
                int weaponSkinValue = rootWeaponSkinPrice.items.Where(s => s.name.ToLower() == itemMarketHash.ToLower()).FirstOrDefault().price;

                //Add tax markup :)
                weaponSkinValue += Convert.ToInt32(weaponSkinValue * float.Parse(SettingsManager.RetrieveFromConfigFile("taxRate")));

                bool userSpecifiedSkinExistsInMarket = false;
                //Make sure skin exists in market
                foreach (var marketSkin in rootWeaponSkins.items)
                {
                    if (marketSkin.market_name.ToLower() == itemMarketHash.ToLower())
                    {
                        userSpecifiedSkinExistsInMarket = true;

                        selectedMarketSkin.Market_hash_name = marketSkin.market_hash_name;
                        selectedMarketSkin.OwnerID = Context.Message.Author.Id;
                        selectedMarketSkin.UnboxDate = DateTime.UtcNow;
                    }
                }

                //Send error if skin does not exist
                if (userSpecifiedSkinExistsInMarket == false)
                {
                    await Context.Message.Channel.SendMessageAsync($"**{Context.Message.Author.ToString().Substring(0, Context.Message.Author.ToString().Length - 5)}**, `{itemMarketHash}` does not exist in the current skin market");                
                }
                //Make sure user has enough credits to buy skin
                else if (UserCreditsHandler.GetUserCredits(Context) < weaponSkinValue)
                {
                    await Context.Message.Channel.SendMessageAsync($"**{Context.Message.Author.ToString().Substring(0, Context.Message.Author.ToString().Length - 5)}**, you do not have enough credits to buy`{itemMarketHash}` | **{UserCreditsHandler.GetUserCredits(Context)} Credits**");
                }
                else
                {
                    //Checks are true, now give user skin and remove credits

                    //Remove user credits
                    UserCreditsHandler.AddCredits(Context, -weaponSkinValue, true);

                    //Add skin to inventory
                    var userSkins = XmlManager.FromXmlFile<UserSkinStorageRootobject>(CoreMethod.GetFileLocation("UserSkinStorage.xml"));

                    userSkins.UserSkinEntries.Add(selectedMarketSkin);

                    var filteredUserSkin = new UserSkinStorageRootobject
                    {
                        SkinAmount = 0,
                        UserSkinEntries = userSkins.UserSkinEntries
                    };

                    XmlManager.ToXmlFile(filteredUserSkin, CoreMethod.GetFileLocation("UserSkinStorage.xml"));

                    //Send receipt
                    await Context.Channel.SendMessageAsync(
                        $"**{Context.Message.Author.ToString().Substring(0, Context.Message.Author.ToString().Length - 5)}**, you bought`{itemMarketHash}`" +
                        $" for **{UserBankingHandler.CreditCurrencyFormatter(weaponSkinValue)} Credits**");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
            }
        }

        public static async Task SellInventoryItemAsync(SocketCommandContext Context, string itemMarketHash)
        {
            //Get price data
            var rootWeaponSkinPrice = CsgoItemPriceHandler.GetRootWeaponSkin();

            try
            {
                int weaponSkinValue = rootWeaponSkinPrice.items.Where(s => s.name.ToLower() == itemMarketHash.ToLower()).FirstOrDefault().price;

                //Give user credits
                UserCreditsHandler.AddCredits(Context, weaponSkinValue, true);

                //Remove skin from inventory
                var userSkin = XmlManager.FromXmlFile<UserSkinStorageRootobject>(CoreMethod.GetFileLocation("UserSkinStorage.xml"));
                var filteredUserSkinEntries = userSkin.UserSkinEntries.Where(s => s.Market_hash_name != itemMarketHash).ToList();

                var filteredUserSkin = new UserSkinStorageRootobject
                {
                    SkinAmount = 0,
                    UserSkinEntries = filteredUserSkinEntries
                };

                XmlManager.ToXmlFile(filteredUserSkin, CoreMethod.GetFileLocation("UserSkinStorage.xml"));

                //Send receipt
                await Context.Channel.SendMessageAsync(
                    $"**{Context.Message.Author.ToString().Substring(0, Context.Message.Author.ToString().Length - 5)}**, you sold your `{itemMarketHash}`" +
                    $" for **{UserBankingHandler.CreditCurrencyFormatter(weaponSkinValue)} Credits** " +
                    $"| A total of **{UserBankingHandler.CreditCurrencyFormatter(UserCreditsTaxHandler.TaxCollector(weaponSkinValue))} Credits was taken off as tax**");
            }
            catch (Exception)
            {
                //Send error if user does not have item
                await Context.Channel.SendMessageAsync($"**{Context.Message.Author.ToString().Substring(0, Context.Message.Author.ToString().Length - 5)}**, you do not have `{itemMarketHash}` in your inventory");
            }

        }

        public static async Task SellAllInventoryItemAsync(SocketCommandContext Context)
        {
            //Get price data
            var rootWeaponSkinPrice = CsgoItemPriceHandler.GetRootWeaponSkin();
            var userSkin = XmlManager.FromXmlFile<UserSkinStorageRootobject>(CoreMethod.GetFileLocation("UserSkinStorage.xml"));

            try
            {
                int weaponSkinValue = 0;


                foreach (var skin in userSkin.UserSkinEntries)
                {
                    try
                    {
                        weaponSkinValue += rootWeaponSkinPrice.items.Where(s => s.name.ToLower() == skin.Market_hash_name.ToLower()).FirstOrDefault().price;
                    }
                    catch (Exception)
                    {
                    }
                    
                }              

                //Give user credits
                UserCreditsHandler.AddCredits(Context, weaponSkinValue, true);

                //Remove skin from inventory
                var filteredUserSkinEntries = userSkin.UserSkinEntries.Where(s => s.OwnerID != Context.Message.Author.Id).ToList();

                var filteredUserSkin = new UserSkinStorageRootobject
                {
                    SkinAmount = 0,
                    UserSkinEntries = filteredUserSkinEntries
                };

                XmlManager.ToXmlFile(filteredUserSkin, CoreMethod.GetFileLocation("UserSkinStorage.xml"));

                //Send receipt
                await Context.Channel.SendMessageAsync(
                    $"**{Context.Message.Author.ToString().Substring(0, Context.Message.Author.ToString().Length - 5)}**, you sold your inventory" +
                    $" for **{UserBankingHandler.CreditCurrencyFormatter(weaponSkinValue)} Credits** " +
                    $"| A total of **{UserBankingHandler.CreditCurrencyFormatter(UserCreditsTaxHandler.TaxCollector(weaponSkinValue))} Credits was taken off as tax**");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Fuck!!!");
                Console.WriteLine(ex.StackTrace);
            }
        }
    }

    public static class CsGoMarketInventoryHandler
    {
        public static PaginatedMessage GetCsgoMarketInventory(SocketCommandContext Context, string filterString)
        {
            //Get price data
            var rootWeaponSkinPrice = CsgoItemPriceHandler.GetRootWeaponSkin();
            var rootWeaponSkin = CsgoCaseUnboxing.CsgoUnboxingHandler.GetRootWeaponSkin();

            List<string> filteredRootWeaponSkin = new List<string>();
            List<string> filteredRootWeaponSkinPrice = new List<string>();

            try
            {
                //Filter rootWeaponSkin to those with a price found in rootWeaponSkinPrice
                foreach (var skin in rootWeaponSkin.items)
                {
                    //If filter string is not null, filter market results by user filter string
                    if ((!string.IsNullOrEmpty(filterString) && skin.market_name.ToLower().Contains(filterString.ToLower())) || (string.IsNullOrEmpty(filterString)))
                    {
                        foreach (var skinPrice in rootWeaponSkinPrice.items)
                        {
                            if (skin.market_hash_name == skinPrice.name)
                            {
                                if (UnicodeLiteralConverter.DecodeToNonAsciiCharacters(skin.market_hash_name) == UnicodeLiteralConverter.DecodeToNonAsciiCharacters(skinPrice.name))
                                {
                                    string skinQualityEmote = "<:white:522875796319240193>";

                                    //Assign quality colors
                                    if (skin.quality_color == "B0C3D9" && skin.name_color == "D2D2D2") skinQualityEmote = "<:white:522875796319240193>"; //white
                                    if (skin.quality_color == "5E98D9" && skin.name_color == "D2D2D2") skinQualityEmote = "<:lightblue:522878230848602131>"; //light blue
                                    if (skin.quality_color == "4B69FF" && skin.name_color == "D2D2D2") skinQualityEmote = "<:darkerblue:522878230550544387>"; //darker blue
                                    if (skin.quality_color == "8847FF" && skin.name_color == "D2D2D2") skinQualityEmote = "<:purple:522878233482625034>"; //purple
                                    if (skin.quality_color == "D32CE6" && skin.name_color == "D2D2D2") skinQualityEmote = "<:pink:522878230856990807>"; //pink
                                    if (skin.quality_color == "EB4B4B" && skin.name_color == "D2D2D2") skinQualityEmote = "<:red:522878230533767199>"; //red
                                    if (skin.quality_color == "EB4B4B" && skin.name_color == "8650AC") skinQualityEmote = "<:gold:522878230634692619>"; //gold
                                    if (skin.quality_color == "E4AE39" && skin.name_color == "D2D2D2") skinQualityEmote = "<:yellowgold:522878230923968513>"; //rare gold

                                    //Add skin entry
                                    try
                                    {
                                        Emote emote = Emote.Parse(skinQualityEmote);

                                        //Add weapon skin
                                        filteredRootWeaponSkin.Add(emote + " " + skin.market_name);

                                        //

                                        //Find weapon skin price for selected wepaon skin
                                        int weaponSkinValue = rootWeaponSkinPrice.items.Where(s => s.name == skin.market_hash_name).FirstOrDefault().price;

                                        //Add tax markup for market item
                                        weaponSkinValue += Convert.ToInt32(weaponSkinValue * float.Parse(SettingsManager.RetrieveFromConfigFile("taxRate")));

                                        //Add weapon skin price
                                        filteredRootWeaponSkinPrice.Add(emote + " " + weaponSkinValue.ToString());
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine(ex.Source);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
            }

            //Configurate paginated message
            var paginationConfig = new PaginationConfig
            {
                AuthorName = "CS:GO Market",
                AuthorURL = Context.Message.Author.GetAvatarUrl(),

                Description = $"Current skin market, to buy skins, use `{MainProgram.botCommandPrefix} case buy [name]`",

                DefaultFieldHeader = "Unable to find specified weapon skin!",
                DefaultFieldDescription = $"Broaden your search parameters and try again",

                Field1Header = "Item Name",
                Field2Header = "Price",
            };

            var paginationManager = new PaginationManager();

            //Generate paginated message
            var pager = paginationManager.GeneratePaginatedMessage(filteredRootWeaponSkin, filteredRootWeaponSkinPrice, paginationConfig);

            return pager;
        }
    }

    public class UserSkinStorageRootobject
    {
        public long SkinAmount { get; set; }
        public List<UserSkinEntry> UserSkinEntries { get; set; }
    }
    public class UserSkinEntry
    {
        public ulong OwnerID { get; set; }
        public string Market_hash_name { get; set; }
        public DateTime UnboxDate { get; set; }
    }
}
