using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
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
    public class CsgoInventoryHandler : InteractiveBase<SocketCommandContext>
    {
        private List<string> embedFields = new List<string>();
        private List<string> embedPriceFields = new List<string>();
        private List<PaginatedMessage.Page> skinPages = new List<PaginatedMessage.Page>();

        public PaginatedMessage DisplayUserCsgoInventory(SocketCommandContext Context)
        {
            //If weapon skin price data is null
            if (CsgoUnboxingHandler.rootWeaponSkin == null)
            {
                //Read skin prices from file
                CsgoUnboxingHandler.GetRootWeaponSkin();
            }

            //Get user skins from xml file
            UserSkinStorageRootobject userSkin = new UserSkinStorageRootobject();
            try
            {
                userSkin = XmlManager.FromXmlFile<UserSkinStorageRootobject>(CoreMethod.GetFileLocation("UserSkinStorage.xml"));
            }
            catch (Exception ex)
            {
                Console.WriteLine("Fuck!!!, unable to read user skin inventory from file");
                Console.WriteLine(ex.StackTrace);
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


            //Generate user page         
            int userSkinsProcessedSinceLastPage = 0;
            foreach (var userSkinEntry in foundUserSkins)
            {
                //number of skin entries before cutting off to a new page
                int entriesPerPage = 15;

                //Create a new page and reset counter if reached 20
                if (userSkinsProcessedSinceLastPage == entriesPerPage)
                {
                    //Add page
                    CreateNewPaginatorPage(embedFields, embedPriceFields, skinPages);

                    //Counter reset
                    userSkinsProcessedSinceLastPage = 0;

                    //Reset fields
                    embedFields = new List<string>();
                    embedPriceFields = new List<string>();
                }

                //Keep adding skins to list if it has not reached cutoff amount
                if (userSkinsProcessedSinceLastPage != entriesPerPage)
                {
                    //Find skin entry info
                    foreach (var storageSkinEntry in CsgoUnboxingHandler.rootWeaponSkin.items)
                    {
                        //Filter by market hash name
                        //LESSON LEARNED: Decode unicode before processing them to avoid them not being recognised!!!!!!!111!!
                        if (UnicodeLiteralConverter.DecodeToNonAsciiCharacters(storageSkinEntry.market_hash_name) == UnicodeLiteralConverter.DecodeToNonAsciiCharacters(userSkinEntry.Market_hash_name))
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
                                embedFields.Add(emote + " "+ storageSkinEntry.market_name);

                                //Get skin data
                                var rootWeaponSkinPrice = CsgoItemPriceHandler.GetRootWeaponSkin();

                                //Filter and Add skin price entry to list
                                embedPriceFields.Add(emote + " " + rootWeaponSkinPrice.items.Where(s => s.name == storageSkinEntry.market_hash_name).FirstOrDefault().price.ToString());
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex.Source);
                            }                            
                        }
                    }

                }

                //Increment counters
                userSkinsProcessedSinceLastPage++;


            }

            //Add blank inline field if user has no skins
            if (embedFields.Count > 0 && embedPriceFields.Count > 0)
            {
                CreateNewPaginatorPage(embedFields, embedPriceFields, skinPages);
            }
            else
            {
                skinPages.Add(new PaginatedMessage.Page
                {
                    Fields = new List<EmbedFieldBuilder>
                    {
                        new EmbedFieldBuilder
                        {
                            Name = "You do not have any skins",
                            Value = $"Go unbox some with `{MainProgram.botCommandPrefix} case open`"
                        }
                    }
                });
            }

            //Create paginated message
            var pager = new PaginatedMessage
            {
                Pages = skinPages,
                Author = new EmbedAuthorBuilder
                {
                    IconUrl = Context.Message.Author.GetAvatarUrl(),
                    Name = Context.Message.Author.ToString().Substring(0, Context.Message.Author.ToString().Length - 5) + " Inventory",
                },
                Color = Color.DarkGreen,
                Description = $"To sell items, use `{MainProgram.botCommandPrefix} case sell [name]`",
                FooterOverride = null,
                //ImageUrl = Context.Client.CurrentUser.GetAvatarUrl(),
                //ThumbnailUrl = Context.Client.CurrentUser.GetAvatarUrl(),
                Options = PaginatedAppearanceOptions.Default,
            };

            return pager;
        }

        private void CreateNewPaginatorPage(List<string> embedFields, List<string> embedPriceFields, List<PaginatedMessage.Page> skinPages)
        {
            skinPages.Add(new PaginatedMessage.Page
            {
                Fields = new List<EmbedFieldBuilder>
                {
                    new EmbedFieldBuilder
                    {
                        Name = "Item Name",
                        Value = string.Join("\n", embedFields),
                        IsInline = true
                    },

                    new EmbedFieldBuilder
                    {
                        Name = "Market Value",
                        Value = string.Join("\n", embedPriceFields),
                        IsInline = true
                    }
                }
            });
        }



    }

    public static class CsgoInventorySaleHandler
    {
        public static void SellInventoryItem(SocketCommandContext Context, string itemMarketHash)
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
                Context.Channel.SendMessageAsync(
                    $"**{Context.Message.Author.ToString().Substring(0, Context.Message.Author.ToString().Length - 5)}**, you sold your `{itemMarketHash}`" +
                    $" for **{UserBankingHandler.CreditCurrencyFormatter(weaponSkinValue)} Credits** " +
                    $"| A total of **{UserBankingHandler.CreditCurrencyFormatter(UserCreditsTaxHandler.TaxCollector(weaponSkinValue))} Credits was taken off as tax**");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
                //Send error if user does not have item
                Context.Channel.SendMessageAsync($"**{Context.Message.Author.ToString().Substring(0, Context.Message.Author.ToString().Length - 5)}**, you do not have `{itemMarketHash}` in your inventory");
            }

        }

        public static void SellAllInventoryItem(SocketCommandContext Context)
        {
            //Get price data
            var rootWeaponSkinPrice = CsgoItemPriceHandler.GetRootWeaponSkin();
            var userSkin = XmlManager.FromXmlFile<UserSkinStorageRootobject>(CoreMethod.GetFileLocation("UserSkinStorage.xml"));

            try
            {
                int weaponSkinValue = 0;

                foreach (var skin in userSkin.UserSkinEntries)
                {
                    weaponSkinValue += rootWeaponSkinPrice.items.Where(s => s.name.ToLower() == skin.Market_hash_name.ToLower()).FirstOrDefault().price;
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
                Context.Channel.SendMessageAsync(
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
