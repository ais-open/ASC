using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Helpers;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using System.Threading.Tasks;
using AzureServiceCatalog.Models;

namespace AzureServiceCatalog.Helpers
{
    public static class BlobHelpers
    {

        public static void CreateInitialTablesAndBlobContainers(string accountName, string key, BaseOperationContext parentOperationContext)
        {
            var thisOperationContext = new BaseOperationContext(parentOperationContext, "BlobHelpers:CreateInitialTablesAndBlobContainers");
            try
            {
                var storageAccount = new CloudStorageAccount(new StorageCredentials(accountName, key), Config.StorageAccountEndpointSuffix, true);
                CreateInitialTables(storageAccount, thisOperationContext);
                CreateInitialBlobContainers(storageAccount, thisOperationContext);
            }
            finally
            {
                thisOperationContext.CalculateTimeTaken();
                TraceHelper.TraceOperation(thisOperationContext);
            }
        }

        public static async Task<string> SaveToBlobContainer(string containerName, Stream stream, string fileExtension, string contentType, BaseOperationContext parentOperationContext, string oldBlobAbsolutePath = null)
        {
            var thisOperationContext = new BaseOperationContext(parentOperationContext, "BlobHelpers:SaveToBlobContainer");
            try
            {
                CloudBlobContainer cloudBlobContainer = await GetOrCreateBlobContainer(containerName, thisOperationContext);

                string blockBlobReference = null;
                if (oldBlobAbsolutePath != null)
                {
                    blockBlobReference = oldBlobAbsolutePath.Split('/').Last();
                }
                if (blockBlobReference == null || blockBlobReference.Split('.').Last() != fileExtension)
                {
                    if (blockBlobReference != null)
                    {
                        cloudBlobContainer.GetBlockBlobReference(blockBlobReference).Delete();
                    }
                    blockBlobReference = Guid.NewGuid().ToString() + "." + fileExtension;
                }
                CloudBlockBlob cloudBlockBlob = cloudBlobContainer.GetBlockBlobReference(blockBlobReference);
                cloudBlockBlob.Properties.ContentType = contentType;
                cloudBlockBlob.UploadFromStream(stream);
                return cloudBlockBlob.Uri.AbsoluteUri;
            }
            finally
            {
                thisOperationContext.CalculateTimeTaken();
                TraceHelper.TraceOperation(thisOperationContext);
            }
        }

        public static string RandomString(int length)
        {
            const string chars = "abcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }
        #region Private Members

        private static async Task<CloudBlobClient> CreateBlobClient(BaseOperationContext parentOperationContext)
        {
            var thisOperationContext = new BaseOperationContext(parentOperationContext, "BlobHelpers:CreateBlobClient");
            try
            {
                var identityHelper = new IdentityHelper();
                var accountName = await identityHelper.GetStorageName(thisOperationContext);
                var accountKey = await identityHelper.GetStorageKey(thisOperationContext);
                var storageAccount = new CloudStorageAccount(new StorageCredentials(accountName, accountKey), Config.StorageAccountEndpointSuffix, true);
                return storageAccount.CreateCloudBlobClient();
            }
            finally
            {
                thisOperationContext.CalculateTimeTaken();
                TraceHelper.TraceOperation(thisOperationContext);
            }
        }

        /// <summary>
        /// Gets a blob container from Azure Blob Storage. If one does not exist with the specified container reference, then one is created.
        /// </summary>
        /// <param name="blobContainerReferenceString">Blob container reference string.</param>
        /// <param name="accessType">Access type. If not specified, the container is made type "Blob", making all contents publicly accessible.</param>
        /// <returns>The retrieved or created CloudBlobContainer.</returns>
        private static async Task<CloudBlobContainer> GetOrCreateBlobContainer(string blobContainerReferenceString, BaseOperationContext parentOperationContext, BlobContainerPublicAccessType accessType = BlobContainerPublicAccessType.Blob)
        {
            var thisOperationContext = new BaseOperationContext(parentOperationContext, "BlobHelpers:GetOrCreateBlobContainer");
            try
            {
                var blobClient = await CreateBlobClient(thisOperationContext);
                var blobContainer = blobClient.GetContainerReference(blobContainerReferenceString);
                blobContainer.CreateIfNotExists(accessType);
                return blobContainer;
            }
            finally
            {
                thisOperationContext.CalculateTimeTaken();
                TraceHelper.TraceOperation(thisOperationContext);
            }
        }

        private static void CreateInitialTables(CloudStorageAccount storageAccount, BaseOperationContext parentOperationContext)
        {
            var thisOperationContext = new BaseOperationContext(parentOperationContext, "BlobHelpers:CreateInitialTables");
            try
            {
                CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
                List<string> initialTableKeys = new List<string>(){
                    Tables.Products,
                    Tables.PolicyLookupPaths
                };
                foreach (string initialTableKey in initialTableKeys)
                {
                    tableClient.GetTableReference(initialTableKey).CreateIfNotExists();
                }
            }
            finally
            {
                thisOperationContext.CalculateTimeTaken();
                TraceHelper.TraceOperation(thisOperationContext);
            }
        }

        private static void CreateInitialBlobContainers(CloudStorageAccount storageAccount, BaseOperationContext parentOperationContext)
        {
            var thisOperationContext = new BaseOperationContext(parentOperationContext, "BlobHelpers:CreateInitialBlobContainers");
            try
            {
                CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
                List<string> initialBlobContainerKeys = new List<string>()
                {
                    BlobContainers.ProductImages
                };
                foreach (string initialBlobContainerKey in initialBlobContainerKeys)
                {
                    CloudBlobContainer cloudBlobContainer = blobClient.GetContainerReference(initialBlobContainerKey);
                    cloudBlobContainer.CreateIfNotExists();
                    cloudBlobContainer.SetPermissions(
                    new BlobContainerPermissions
                    {
                        PublicAccess = BlobContainerPublicAccessType.Blob
                    });
                }
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