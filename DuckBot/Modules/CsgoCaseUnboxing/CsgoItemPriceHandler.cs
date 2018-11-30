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
    public class CsgoItemPriceHandler
    {
        public SkinItem GetItemPrice(string marketHashName)
        {
            var rootObject = GetRootWeaponSkinPrice();

            var sortedResult = rootObject.items
                .Where(e => !e.market_name.ToLower().Contains(marketHashName)).ToArray();


            var returnResult = sortedResult[0];

            return returnResult;
        }

        private static RootWeaponSkin GetRootWeaponSkinPrice()
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
}
