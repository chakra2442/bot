using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace R3DD17
{
    public class Stats : TableEntity
    {
        public double Score { get; set; }
        public string Sentiments { get; set; }
        public string Name { get; set; }
        public double Change { get; set; }
        public string MarketCap { get; set; }
        public double LastPrice { get; set; }

        public Stats()
        {
        }
        public Stats(string sym)
        {
            this.RowKey = sym;
            this.PartitionKey = sym;
        }
        
        public static Stats CalculateStats(string sym, List<TextBlob> entries)
        {
            var avgScore = Math.Round(entries.Sum(x => x.Score) / entries.Count * 100.0, 1);
            var sentiments = new Dictionary<string, List<Tuple<string, DateTime>>>();

            foreach (var entry in entries)
            {
                if (!sentiments.ContainsKey(entry.Sentiment))
                {
                    sentiments.Add(entry.Sentiment, new List<Tuple<string, DateTime>>());
                }

                sentiments[entry.Sentiment].Add(new Tuple<string, DateTime>(entry.RawText, DateTime.Parse(entry.Timestamp)));
            }

            StringBuilder str = new StringBuilder();
            foreach (var item in sentiments.OrderByDescending(x => x.Key))
            {
                var topSamples = item.Value.OrderByDescending(x => x.Item2).Take(3).Select(x => x.Item1);
                str.Append($"<b>{item.Key}</b><ul><li>{string.Join("</li><li>", topSamples)}</li></ul>");
            }

            var stat = new Stats(sym);
            stat.Score = avgScore;
            stat.Sentiments = str.ToString().Trim();

            if (NASDAQ.allTickers.ContainsKey(sym) && !string.IsNullOrEmpty(NASDAQ.allTickers[sym].name))
            {
                try
                {
                    stat.Name = NASDAQ.allTickers[sym].name;
                    stat.Change = double.Parse(NASDAQ.allTickers[sym].pctchange.TrimEnd(new char[] { '%', ' ' })) / 100;
                    var mktCap = long.Parse(NASDAQ.allTickers[sym].marketCap, System.Globalization.NumberStyles.AllowThousands) / 1000000;
                    if (mktCap > 1000000)
                    {
                        stat.MarketCap = $"{(mktCap / 1000000.0).ToString("###.#")}T";
                    }
                    else if (mktCap > 1000)
                    {
                        stat.MarketCap = $"{(mktCap / 1000.0).ToString("###.#")}B";
                    }
                    else
                    {
                        stat.MarketCap = $"{mktCap.ToString("###.#")}M";
                    }

                    stat.LastPrice = double.Parse(NASDAQ.allTickers[sym].lastsale.TrimStart(new char[] { '$', ' ' }));
                }
                catch(Exception ex)
                {
                    Console.WriteLine($"{sym} : Aggregation error : {ex}");
                    return null;
                }
            }
            else
            {
                return null;
            }

            return stat;
        }
    }
}



