using AzureServiceCatalog.Models;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;

namespace AzureServiceCatalog.Helpers.BudgetHelper
{
    public class AzureTableRepository<T> : IAzureTableRepository<T> where T : ITableEntity, new()
    {
        private string tableName;

        public AzureTableRepository(string tblName)
        {
            tableName = tblName;
        }

        private async Task<CloudTable> GetTableAsync()
        {
            var thisOperationContext = new BaseOperationContext("AzureTableRepository:GetTableAsync");
            var identityHelper = new IdentityHelper();
            var accountName = await identityHelper.GetStorageName(thisOperationContext);
            var accountKey = await identityHelper.GetStorageKey(thisOperationContext);
            CloudStorageAccount storageAccount = new CloudStorageAccount(new StorageCredentials(accountName, accountKey), Config.StorageAccountEndpointSuffix, true);
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
            CloudTable table = tableClient.GetTableReference(tableName);
            await table.CreateIfNotExistsAsync();

            return table;
        }

        public async Task<List<T>> GetList(TableQuery<T> query) 
        {
            CloudTable table = await GetTableAsync();

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

        public async Task<T> GetSingle(string partitionKey, string rowKey)
        {
            CloudTable table = await GetTableAsync();
            TableOperation operation = TableOperation.Retrieve<T>(partitionKey, rowKey);
            TableResult result = await table.ExecuteAsync(operation);

            return (T)(dynamic)result.Result;
        }

        public async Task Add(T entity)
        {
            CloudTable table = await GetTableAsync();
            TableOperation operation = TableOperation.Insert(entity);
            await table.ExecuteAsync(operation);
        }

        public async Task Update(T entity)
        {
            CloudTable table = await GetTableAsync();
            TableOperation operation = TableOperation.InsertOrReplace(entity);
            await table.ExecuteAsync(operation);
        }

        public async Task Delete(string partitionKey, string rowKey)
        {
            T entity = await GetSingle(partitionKey, rowKey);
            CloudTable table = await GetTableAsync();
            TableOperation operation = TableOperation.Delete(entity);
            await table.ExecuteAsync(operation);
        }
    }
}
