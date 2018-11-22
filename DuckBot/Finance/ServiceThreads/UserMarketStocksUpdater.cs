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
                //Get market stocks
                var marketStockStorage = XmlManager.FromXmlFile<MarketStockStorage>(CoreMethod.GetFileLocation(@"\MarketStocksValue.xml"));

                List<MarketStock> updatedMarketStocks = new List<MarketStock>();

                //Get real price for each
                foreach (var stock in marketStockStorage.MarketStock)
                {
                    long stockPriceNew = Convert.ToInt64(OnlineStockHandler.GetOnlineStockInfo(stock.StockTicker).LatestPrice * 100);

                    updatedMarketStocks.Add(new MarketStock {StockTicker = stock.StockTicker, StockPrice = stockPriceNew });
                }

                //Write to file
                var marketStock = new MarketStockStorage
                {
                    MarketOpen = OnlineStockHandler.GetOnlineIsOpen(),
                    MarketStock = updatedMarketStocks
                };

                XmlManager.ToXmlFile(marketStock, CoreMethod.GetFileLocation(@"\MarketStocksValue.xml"));

                //Wait 10 seconds
                Thread.Sleep(10000);
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

            if (returnStockInfo.LatestSource == "Close")
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
        public string Symbol { get; set; }
        public string CompanyName { get; set; }
        public string PrimaryExchange { get; set; }
        public string Sector { get; set; }
        public string CalculationPrice { get; set; }
        public double Open { get; set; }
        public long OpenTime { get; set; }
        public double Close { get; set; }
        public long CloseTime { get; set; }
        public double High { get; set; }
        public double Low { get; set; }
        public double LatestPrice { get; set; }
        public string LatestSource { get; set; }
        public string LatestTime { get; set; }
        public long LatestUpdate { get; set; }
        public double LatestVolume { get; set; }
        public string IexRealtimePrice { get; set; }
        public string IexRealtimeSize { get; set; }
        public string IexLastUpdated { get; set; }
        public double DelayedPrice { get; set; }
        public long DelayedPriceTime { get; set; }
        public double PreviousClose { get; set; }
        public double Change { get; set; }
        public double ChangePercent { get; set; }
        public string IexMarketPercent { get; set; }
        public string IexVolume { get; set; }
        public double AvgTotalVolume { get; set; }
        public string IexBidPrice { get; set; }
        public string IexBidSize { get; set; }
        public string IexAskPrice { get; set; }
        public string IexAskSize { get; set; }
        public long MarketCap { get; set; }
        public double PeRatio { get; set; }
        public double Week52High { get; set; }
        public double Week52Low { get; set; }
        public double YtdChange { get; set; }
    }
}
