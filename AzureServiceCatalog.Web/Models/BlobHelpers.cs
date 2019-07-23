using System;
using System.Collections.Generic;
using System.Configuration;
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

namespace AzureServiceCatalog.Web.Models
{
    public static class BlobHelpers
    {

        public static void CreateInitialTablesAndBlobContainers(string accountName, string key)
        {
            var storageAccount = new CloudStorageAccount(new StorageCredentials(accountName, key), Config.StorageAccountEndpointSuffix, true);
            CreateInitialTables(storageAccount);
            CreateInitialBlobContainers(storageAccount);
        }

        public static async Task<string> SaveToBlobContainer(string containerName, Stream stream, string fileExtension, string contentType, string oldBlobAbsolutePath = null)
        {
            CloudBlobContainer cloudBlobContainer = await GetOrCreateBlobContainer(containerName);

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

        public static string RandomString(int length)
        {
            const string chars = "abcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }
        #region Private Members

        private static async Task<CloudBlobClient> CreateBlobClient()
        {
            var identityModels = new IdentityModels();
            var accountName = await identityModels.GetStorageName();
            var accountKey = await identityModels.GetStorageKey();
            var storageAccount = new CloudStorageAccount(new StorageCredentials(accountName, accountKey), Config.StorageAccountEndpointSuffix, true);
            return storageAccount.CreateCloudBlobClient();
        }
        /// <summary>
        /// Gets a blob container from Azure Blob Storage. If one does not exist with the specified container reference, then one is created.
        /// </summary>
        /// <param name="blobContainerReferenceString">Blob container reference string.</param>
        /// <param name="accessType">Access type. If not specified, the container is made type "Blob", making all contents publicly accessible.</param>
        /// <returns>The retrieved or created CloudBlobContainer.</returns>
        private static async Task<CloudBlobContainer> GetOrCreateBlobContainer(string blobContainerReferenceString, BlobContainerPublicAccessType accessType = BlobContainerPublicAccessType.Blob)
        {
            var blobClient = await CreateBlobClient();
            var blobContainer = blobClient.GetContainerReference(blobContainerReferenceString);
            blobContainer.CreateIfNotExists(accessType);
            return blobContainer;
        }

        private static void CreateInitialTables(CloudStorageAccount storageAccount)
        {
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
            List<string> initialTableKeys = new List<string>()
            {
                Tables.Products,
                Tables.PolicyLookupPaths
            };
            foreach (string initialTableKey in initialTableKeys)
            {
                tableClient.GetTableReference(initialTableKey).CreateIfNotExists();
            }
        }
        private static void CreateInitialBlobContainers(CloudStorageAccount storageAccount)
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

        #endregion

    }
}