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
    public static class CsgoItemPriceHandler
    {
        private static Random rand = new Random();

        public static RootWeaponSkinPrice rootWeaponSkinPrice;

        /// <summary>
        /// Returns the market value of the skin, multiplied by 100
        /// </summary>
        /// <param name="skinName">Name of the skin to find</param>
        /// <returns>Long of the skin price mutltiplied by 100</returns>
        public static long GetWeaponSkinPrice(string skinName)
        {
            //Read skin prices from file
            GetRootWeaponSkin();

            //Filter to ones with the skinName
            try
            {
                var sortedWeaponSkinPrice = rootWeaponSkinPrice.items.Where(s => s.name == skinName).ToArray();

                var sortedWeaponSkinSingularPrice = sortedWeaponSkinPrice.FirstOrDefault();

                //Return the price
                return sortedWeaponSkinSingularPrice.price;
            }
            //If it failed to find skin price
            catch (Exception)
            {
                //Return random amount
                return rand.Next(50, 200);
            }

        }

        public static RootWeaponSkinPrice GetRootWeaponSkin()
        {
            if (rootWeaponSkinPrice == null)
            {
                //Read skin data from local json file
                using (StreamReader r = new StreamReader(CoreMethod.GetFileLocation("skinPricing.json")))
                {
                    string json = r.ReadToEnd();
                    rootWeaponSkinPrice = JsonConvert.DeserializeObject<RootWeaponSkinPrice>(json);
                }
            }

            return rootWeaponSkinPrice;
        }
    }

    public class Item
    {
        public string name { get; set; }
        public int price { get; set; }
        public int have { get; set; }
        public int max { get; set; }
        public int rate { get; set; }
        public int tr { get; set; }
        public int res { get; set; }
    }

    public class RootWeaponSkinPrice
    {
        public bool success { get; set; }
        public int num_items { get; set; }
        public List<Item> items { get; set; }
    }
}
