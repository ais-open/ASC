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
    public static class TableHelper
    {
        internal static async Task SaveCoreTableItem<T>(T item, string tableName, BaseOperationContext parentOperationContext) where T : TableEntity
        {
            var thisOperationContext = new BaseOperationContext(parentOperationContext, "TableHelper:SaveCoreTableItem");
            try
            {
                var table = TableHelper.GetCoreTableReference(tableName, thisOperationContext);
                var saveOperation = TableOperation.InsertOrReplace(item);
                await table.ExecuteAsync(saveOperation);
            }
            finally
            {
                thisOperationContext.CalculateTimeTaken();
                TraceHelper.TraceOperation(thisOperationContext);
            }
        }

        internal static async Task<T> SaveTableItem<T>(T item, string tableName, BaseOperationContext parentOperationContext) where T : TableEntity
        {
            var thisOperationContext = new BaseOperationContext(parentOperationContext, "TableHelper:SaveTableItem");
            try
            {
                var table = await TableHelper.GetTableReference(tableName, thisOperationContext);
                var saveOperation = TableOperation.InsertOrReplace(item);
                var tableResult = await table.ExecuteAsync(saveOperation);
                return (T)tableResult.Result;
            }
            finally
            {
                thisOperationContext.CalculateTimeTaken();
                TraceHelper.TraceOperation(thisOperationContext);
            }
        }

        internal static void SaveCoreTableItemSync<T>(T item, string tableName, BaseOperationContext parentOperationContext) where T : TableEntity
        {
            var thisOperationContext = new BaseOperationContext(parentOperationContext, "TableHelper:SaveCoreTableItemSync");
            try
            {
                var table = TableHelper.GetCoreTableReference(tableName, thisOperationContext);
                var saveOperation = TableOperation.InsertOrReplace(item);
                table.Execute(saveOperation);
            }
            finally
            {
                thisOperationContext.CalculateTimeTaken();
                TraceHelper.TraceOperation(thisOperationContext);
            }
        }

        internal static async Task DeleteCoreTableItem<T>(T item, string tableName, BaseOperationContext parentOperationContext) where T : TableEntity
        {
            var thisOperationContext = new BaseOperationContext(parentOperationContext, "TableHelper:DeleteCoreTableItem");
            try
            {
                var table = TableHelper.GetCoreTableReference(tableName, thisOperationContext);
                var deleteOperation = TableOperation.Delete(item);
                await table.ExecuteAsync(deleteOperation);
            }
            finally
            {
                thisOperationContext.CalculateTimeTaken();
                TraceHelper.TraceOperation(thisOperationContext);
            }
        }

        internal static async Task DeleteTableItem<T>(T item, string tableName, BaseOperationContext parentOperationContext) where T : TableEntity
        {
            var thisOperationContext = new BaseOperationContext(parentOperationContext, "TableHelper:DeleteTableItem");
            try
            {
                item.PartitionKey = item.PartitionKey ?? "Admin";
                item.ETag = "*";
                var table = await TableHelper.GetTableReference(tableName, thisOperationContext);
                var deleteOperation = TableOperation.Delete(item);
                await table.ExecuteAsync(deleteOperation);
            }
            finally
            {
                thisOperationContext.CalculateTimeTaken();
                TraceHelper.TraceOperation(thisOperationContext);
            }
        }

        internal static CloudTable GetCoreTableReference(string tableName, BaseOperationContext parentOperationContext)
        {
            var thisOperationContext = new BaseOperationContext(parentOperationContext, "TableHelper:GetCoreTableReference");
            try
            {
                var tableClient = CreateCoreTableClient(thisOperationContext);
                var tableRef = tableClient.GetTableReference(tableName);
                tableRef.CreateIfNotExists();
                return tableRef;
            }
            finally
            {
                thisOperationContext.CalculateTimeTaken();
                TraceHelper.TraceOperation(thisOperationContext);
            }
        }

        internal static async Task<CloudTable> GetTableReference(string tableName, BaseOperationContext parentOperationContext)
        {
            var thisOperationContext = new BaseOperationContext(parentOperationContext, "TableHelper:GetTableReference");
            try
            {
                var tableClient = await CreateTableClient(thisOperationContext);
                var tableRef = tableClient.GetTableReference(tableName);
                tableRef.CreateIfNotExists();
                return tableRef;
            }
            finally
            {
                thisOperationContext.CalculateTimeTaken();
                TraceHelper.TraceOperation(thisOperationContext);
            }
        }

        #region Private Members

        private static async Task<CloudTableClient> CreateTableClient(BaseOperationContext parentOperationContext)
        {
            var thisOperationContext = new BaseOperationContext(parentOperationContext, "TableHelper:CreateTableClient");
            try
            {
                var identityHelper = new IdentityHelper();
                var accountName = await identityHelper.GetStorageName(thisOperationContext);
                var accountKey = await identityHelper.GetStorageKey(thisOperationContext);
                var storageAccount = new CloudStorageAccount(new StorageCredentials(accountName, accountKey), Config.StorageAccountEndpointSuffix, true);

                CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
                return tableClient;
            }
            finally
            {
                thisOperationContext.CalculateTimeTaken();
                TraceHelper.TraceOperation(thisOperationContext);
            }
        }

        private static CloudTableClient CreateCoreTableClient(BaseOperationContext parentOperationContext)
        {
            var thisOperationContext = new BaseOperationContext(parentOperationContext, "TableHelper:CreateCoreTableClient");
            try
            {
                var accountName = ConfigurationManager.AppSettings["StorageAccountName"];
                var accountKey = ConfigurationManager.AppSettings["StorageAccountKey"];
                var storageAccount = new CloudStorageAccount(new StorageCredentials(accountName, accountKey), Config.StorageAccountEndpointSuffix, true);

                CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
                return tableClient;
            }
            finally
            {
                thisOperationContext.CalculateTimeTaken();
                TraceHelper.TraceOperation(thisOperationContext);
            }
        }

        #endregion
    }
}