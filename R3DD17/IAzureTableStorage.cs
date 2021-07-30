using Microsoft.WindowsAzure.Storage.Table;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace R3DD17
{
    public interface IAzureTableStorage<T> where T : TableEntity, new()
    {
        Task Delete(string partitionkey, string rowKey);
        Task<T> GetItem(string partitionkey, string rowKey);
        Task<List<T>> GetList();
        Task Insert(T item);
        Task Update(T item);
    }
}
