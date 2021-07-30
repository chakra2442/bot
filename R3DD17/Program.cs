using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace R3DD17
{
    class Program
    {
        static void Main(string[] args)
        {
            NASDAQ.Load();
            Fetch().Wait();
            Aggregate().Wait();
        }

        public static async Task Fetch()
        {
            var crawler = new Crawler();
            var results = new List<TextBlob>();
            var tags = ConfigurationManager.AppSettings["Subreddits"].Split(';');
            var count = 100;

            foreach (var tag in tags)
            {
                var crawledResult = crawler.GetTop(tag, count);
                char[] delimiterChars = { ' ', ',', '.', ':', '\t' };
                foreach (var crawledTxt in crawledResult)
                {
                    var words = crawledTxt.Split(delimiterChars, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var word in words)
                    { 
                        var sym = NASDAQ.IsTicker(word);
                        if (sym != null)
                        {
                            results.Add(new TextBlob(sym, crawledTxt));
                        }
                    }
                }
            }

            // WriteToFile(results);
            await WriteToTable(results);
        }

        
        private static async Task WriteToTable(List<TextBlob> results)
        {
            var nlp = new VaderSentiment();
            var settings = new AzureTableSettings(ConfigurationManager.AppSettings["ConnectionString"], ConfigurationManager.AppSettings["RawRecordsTableName"]);
            var storage = new AzureTableStorage<TextBlob>(settings);
            foreach (var txtItem in results)
            {
                try
                {
                    var cachedItem = await storage.GetItem(txtItem.PartitionKey, txtItem.RowKey);
                    if (cachedItem == null)
                    {
                        var parsed = nlp.Parse(txtItem.RawText);
                        txtItem.Sentiment = parsed.Sentiment.Value;
                        if (txtItem.Sentiment == "Positive")
                        {
                            txtItem.Score = 1.0 * parsed.Sentiment.ConfidenceScore;
                        }
                        if (txtItem.Sentiment == "Neutral" || txtItem.Sentiment == "Mixed")
                        {
                            txtItem.Score = 0.5 * parsed.Sentiment.ConfidenceScore;
                        }

                        Console.WriteLine($"NL : {txtItem.PartitionKey} : {txtItem.Sentiment}");
                        await storage.Insert(txtItem);
                    }
                    else
                    {
                        Console.WriteLine($"Cached : {cachedItem.PartitionKey} : {cachedItem.Sentiment}");
                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine($"{txtItem.PartitionKey}/{txtItem.RowKey}: {ex}");
                }
            }
        }

        public static async Task Aggregate()
        {
            var settingsRawRecords = new AzureTableSettings(ConfigurationManager.AppSettings["ConnectionString"], ConfigurationManager.AppSettings["RawRecordsTableName"]);
            var storageRawRecords = new AzureTableStorage<TextBlob>(settingsRawRecords);

            var settingsAgg = new AzureTableSettings(ConfigurationManager.AppSettings["ConnectionString"], ConfigurationManager.AppSettings["AggTableName"]);
            var storageAgg = new AzureTableStorage<Stats>(settingsAgg);


            var rawResults = await storageRawRecords.GetList();
            var grouped = rawResults.GroupBy(x => x.PartitionKey).Select(x => Tuple.Create(x.Key, x.ToList()));
            var aggStats = new List<Stats>();
            foreach (var sym in grouped)
            {
                var stat = Stats.CalculateStats(sym.Item1, sym.Item2);
                if (stat != null)
                {
                    aggStats.Add(stat);
                }
            }

            var sorted = aggStats.OrderByDescending(x => x.Score);
            foreach (var item in sorted)
            {
                Console.WriteLine(item.RowKey + "=>" + item.Score + $"[{item.Sentiments}]");
                await storageAgg.Update(item);
            }
        }

        private static void WriteToFile(List<TextBlob> results)
        {
            File.WriteAllLines("D:\\test.txt", results.Select(x => $"{x.PartitionKey} ==> {x.RawText}"));
        }
        private static void CleanupCheck()
        {
            var sample = new List<string>() { "WHEN DO YOU SELL A GAIN??????" };
            var crawler = new Crawler();
            crawler.CleanUp("test", sample);
        }
    }
}



