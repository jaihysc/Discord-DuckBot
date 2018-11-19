using DuckBot_ClassLibrary;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DuckBot.Finance.ServiceThreads
{
    public class MarketStockStorage
    {
        public bool MarketOpen { get; set; }
        public List<MarketStock> MarketStock { get; set; }
    }
    public class MarketStock
    {
        public string StockTicker { get; set; }
        public long StockPrice { get; set; }
    }

    public class UserMarketStocksUpdater
    {
        //User override values
        public static bool overrideMarketDirection = false;
        public static int marketDirection = 0;
        public static int randNextMax = 2;

        /// <summary>
        /// Updates the user market stocks
        /// </summary>
        public static void UpdateMarketStocks()
        {           
            while (MainProgram._stopThreads == false)
            {
                var marketStockStorage = XmlManager.FromXmlFile<MarketStockStorage>(TaskMethods.GetFileLocation(@"\MarketStocksValue.xml"));

                List<MarketStock> updatedMarketStocks = new List<MarketStock>();

                foreach (var stock in marketStockStorage.MarketStock)
                {
                    long stockPriceNew = Convert.ToInt64(OnlineStockHandler.GetOnlineStockInfo(stock.StockTicker).latestPrice * 100);

                    updatedMarketStocks.Add(new MarketStock {StockTicker = stock.StockTicker, StockPrice = stockPriceNew });
                }

                var marketStock = new MarketStockStorage
                {
                    MarketOpen = OnlineStockHandler.GetOnlineIsOpen(),
                    MarketStock = updatedMarketStocks
                };

                XmlManager.ToXmlFile(marketStock, TaskMethods.GetFileLocation(@"\MarketStocksValue.xml"));

                //Wait 3 seconds
                Thread.Sleep(3000);
            }
        }
    }

    public class OnlineStockHandler
    {
        public static CompanyInfoResponse GetOnlineStockInfo(string symbol)
        {
            var IEXTrading_API_PATH = "https://api.iextrading.com/1.0/stock/{0}/quote";

            IEXTrading_API_PATH = string.Format(IEXTrading_API_PATH, symbol);

            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                //For IP-API
                client.BaseAddress = new Uri(IEXTrading_API_PATH);
                HttpResponseMessage response = client.GetAsync(IEXTrading_API_PATH).GetAwaiter().GetResult();
                if (response.IsSuccessStatusCode)
                {
                    var companysInfo = response.Content.ReadAsAsync<CompanyInfoResponse>().GetAwaiter().GetResult();
                    if (companysInfo != null)
                    {
                        return companysInfo;
                    }
                }

                return null;
            }
        }

        public static bool GetOnlineIsOpen()
        {
            var returnStockInfo = GetOnlineStockInfo("aapl");

            if (returnStockInfo.latestSource == "Close")
            {
                return false;
            }
            else
            {
                return true;
            }
        }
    }


    public class CompanyInfoResponse
    {
        public string symbol { get; set; }
        public string companyName { get; set; }
        public string primaryExchange { get; set; }
        public string sector { get; set; }
        public string calculationPrice { get; set; }
        public double open { get; set; }
        public long openTime { get; set; }
        public double close { get; set; }
        public long closeTime { get; set; }
        public double high { get; set; }
        public double low { get; set; }
        public double latestPrice { get; set; }
        public string latestSource { get; set; }
        public string latestTime { get; set; }
        public long latestUpdate { get; set; }
        public double latestVolume { get; set; }
        public string iexRealtimePrice { get; set; }
        public string iexRealtimeSize { get; set; }
        public string iexLastUpdated { get; set; }
        public double delayedPrice { get; set; }
        public long delayedPriceTime { get; set; }
        public double previousClose { get; set; }
        public double change { get; set; }
        public double changePercent { get; set; }
        public string iexMarketPercent { get; set; }
        public string iexVolume { get; set; }
        public double avgTotalVolume { get; set; }
        public string iexBidPrice { get; set; }
        public string iexBidSize { get; set; }
        public string iexAskPrice { get; set; }
        public string iexAskSize { get; set; }
        public long marketCap { get; set; }
        public double peRatio { get; set; }
        public double week52High { get; set; }
        public double week52Low { get; set; }
        public double ytdChange { get; set; }
    }
}
