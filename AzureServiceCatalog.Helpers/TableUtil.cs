using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.WindowsAzure.Storage.Blob;
using AzureServiceCatalog.Models;

namespace AzureServiceCatalog.Helpers
{
    /// <summary>
    /// Provides common functions for the TableRepository and the TableCoreRepository
    /// </summary>
    public static class TableUtil
    {
        internal static async Task SaveCoreTableItem<T>(T item, string tableName) where T : TableEntity
        {
            var table = TableUtil.GetCoreTableReference(tableName);
            var saveOperation = TableOperation.InsertOrReplace(item);
            await table.ExecuteAsync(saveOperation);
        }

        internal static async Task<T> SaveTableItem<T>(T item, string tableName) where T : TableEntity
        {
            var table = await TableUtil.GetTableReference(tableName);
            var saveOperation = TableOperation.InsertOrReplace(item);
            var tableResult = await table.ExecuteAsync(saveOperation);
            return (T)tableResult.Result;
        }

        internal static void SaveCoreTableItemSync<T>(T item, string tableName) where T : TableEntity
        {
            var table = TableUtil.GetCoreTableReference(tableName);
            var saveOperation = TableOperation.InsertOrReplace(item);
            table.Execute(saveOperation);
        }
        internal static async Task DeleteCoreTableItem<T>(T item, string tableName) where T : TableEntity
        {
            var table = TableUtil.GetCoreTableReference(tableName);
            var deleteOperation = TableOperation.Delete(item);
            await table.ExecuteAsync(deleteOperation);
        }

        internal static async Task DeleteTableItem<T>(T item, string tableName) where T : TableEntity
        {
            item.PartitionKey = item.PartitionKey ?? "Admin";
            item.ETag = "*";
            var table = await TableUtil.GetTableReference(tableName);
            var deleteOperation = TableOperation.Delete(item);
            await table.ExecuteAsync(deleteOperation);
        }

        internal static CloudTable GetCoreTableReference(string tableName)
        {
            var tableClient = CreateCoreTableClient();
            var tableRef = tableClient.GetTableReference(tableName);
            tableRef.CreateIfNotExists();
            return tableRef;
        }

        internal static async Task<CloudTable> GetTableReference(string tableName)
        {
            var tableClient = await CreateTableClient();
            var tableRef = tableClient.GetTableReference(tableName);
            tableRef.CreateIfNotExists();
            return tableRef;
        }

        #region Private Members

        private static async Task<CloudTableClient> CreateTableClient()
        {
            var identityHelper = new IdentityHelper();
            var accountName = await identityHelper.GetStorageName();
            var accountKey = await identityHelper.GetStorageKey();
            var storageAccount = new CloudStorageAccount(new StorageCredentials(accountName, accountKey), Config.StorageAccountEndpointSuffix, true);

            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
            return tableClient;
        }

        private static CloudTableClient CreateCoreTableClient()
        {
            var accountName = ConfigurationManager.AppSettings["StorageAccountName"];
            var accountKey = ConfigurationManager.AppSettings["StorageAccountKey"];
            var storageAccount = new CloudStorageAccount(new StorageCredentials(accountName, accountKey), Config.StorageAccountEndpointSuffix, true);

            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
            return tableClient;
        }

        #endregion
    }
}