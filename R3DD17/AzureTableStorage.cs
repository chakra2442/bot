using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace R3DD17
{
    public class AzureTableStorage<T> : IAzureTableStorage<T> where T : TableEntity, new()
    {
        private readonly AzureTableSettings settings;
        public AzureTableStorage(AzureTableSettings settings)
        {
            this.settings = settings;
        }
        public async Task<List<T>> GetList()
        {
            CloudTable table = await GetTableAsync();
            TableQuery<T> query = new TableQuery<T>();
            List<T> results = new List<T>();
            TableContinuationToken continuationToken = null;
            do
            {
                TableQuerySegment<T> queryResults = await table.ExecuteQuerySegmentedAsync(query, continuationToken);
                continuationToken = queryResults.ContinuationToken;
                results.AddRange(queryResults.Results);
            } while (continuationToken != null);
            return results;
        }
        public async Task<T> GetItem(string partitionKey, string rowKey)
        {
            CloudTable table = await GetTableAsync();
            TableOperation operation = TableOperation.Retrieve<T>(partitionKey, rowKey);
            TableResult result = await table.ExecuteAsync(operation);
            return (T)(dynamic)result.Result;
        }
        public async Task Insert(T item)
        {
            CloudTable table = await GetTableAsync();
            TableOperation operation = TableOperation.Insert(item);
            await table.ExecuteAsync(operation);
        }
        public async Task Update(T item)
        {
            CloudTable table = await GetTableAsync();
            TableOperation operation = TableOperation.InsertOrReplace(item);
            await table.ExecuteAsync(operation);
        }
        public async Task Delete(string partitionkey, string rowKey)
        {
            T item = await GetItem(partitionkey, rowKey);
            CloudTable table = await GetTableAsync();
            TableOperation operation = TableOperation.Delete(item);
            await table.ExecuteAsync(operation);
        }
        private async Task<CloudTable> GetTableAsync()
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(this.settings.ConnectionString);
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
            CloudTable table = tableClient.GetTableReference(this.settings.TableName);
            await table.CreateIfNotExistsAsync();
            return table;
        }
    }
}
