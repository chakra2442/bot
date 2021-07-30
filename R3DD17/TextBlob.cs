using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Security.Cryptography;
using System.Text;

namespace R3DD17
{
    public class TextBlob : TableEntity
    {
        public TextBlob()
        {
        }
        
        public TextBlob(string sym, string item)
        {
            this.RawText = item;
            this.Score = 0;
            this.Sentiment = string.Empty;
            this.PartitionKey = sym;
            this.RowKey = CreateMD5(item);
        }

        public string RawText { get; set; }
        public string Sentiment { get; set; }
        public double Score { get; set; }
        public string Timestamp => DateTime.UtcNow.ToString("u");

        private static string CreateMD5(string input)
        {
            using (var md5 = MD5.Create())
            {
                byte[] inputBytes = Encoding.ASCII.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                // Convert the byte array to hexadecimal string
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("X2"));
                }
                return sb.ToString();
            }
        }
    }
}



