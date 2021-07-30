using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace R3DD17
{
    public static class NASDAQ
    {
        static public Dictionary<string, Ticker> allTickers = new Dictionary<string, Ticker>();

        public static void Load()
        {
            using (var client = new HttpClient())
            {
                var request = new HttpRequestMessage(HttpMethod.Get, "https://api.nasdaq.com/api/screener/stocks?tableonly=true&limit=10000");
                request.Headers.UserAgent.Add(new ProductInfoHeaderValue("PostmanRuntime", "7.26"));
                var result = client.SendAsync(request).Result;
                if (result.IsSuccessStatusCode)
                {
                    var resp = JsonConvert.DeserializeObject<dynamic>(result.Content.ReadAsStringAsync().Result);
                    var nasdaqTickers = (List<Ticker>)JsonConvert.DeserializeObject<List<Ticker>>(resp["data"]["table"]["rows"].ToString());
                    foreach (var tk in nasdaqTickers)
                    {
                        long mktCap;
                        if(long.TryParse(tk.marketCap, NumberStyles.AllowThousands, CultureInfo.InvariantCulture,out mktCap) && mktCap > 100000000 && tk.symbol.Length >= 2)
                        {
                            allTickers.Add(tk.symbol, tk);
                        }
                    }

                    Console.WriteLine($"Loaded {allTickers.Count} tickers symbols");
                }
                else
                {
                    Console.WriteLine("Failed to download tickers");
                }
            }
        }

        public static string IsTicker(string word)
        {
            var blacklistedTickers = ConfigurationManager.AppSettings["BlackListedTickers"].Split(';').ToList();
            var sym = word.Trim().TrimStart('$').TrimStart('(').TrimEnd(')');
            if (IsAlphaNumeric(sym) && allTickers.ContainsKey(sym) && !blacklistedTickers.Contains(sym))
            {
                return sym;
            }

            return null;
        }

        private static bool IsAlphaNumeric(string strToCheck)
        {
            var rg = new Regex(@"^[a-zA-Z0-9\s,]*$");
            return rg.IsMatch(strToCheck);
        }
    }

    public class responseObj
    {

    }
       
    public class Ticker
    {
        public string symbol;
        public string name;
        public string lastsale;
        public string netchange;
        public string pctchange;
        public string marketCap;
        public string url;
    }
}
