using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Diagnostics;
using System.IO;
using System.Security.Claims;
using Newtonsoft.Json;
using Wintellect.Azure.Storage.Table;

namespace AzureServiceCatalog.Web.Models
{
    /// <summary>
    /// This repository handles data access for the "non-Core" tables. The non-Core tables are the ones that are stored in the
    /// user's own storage account in their subscription, as opposed to the "Core" tables which are stored in the actual
    /// Service Catalog's storage account.
    /// </summary>
    public class TableRepository
    {
        private const string partitionKey = "Admin";

        /// <summary>
        /// This implementation uses Wintellect Azure Table library to overcome the limitation of 32KB for string attributes.
        /// The ARM template data is now stored as bytes and Wintellect library is used to automatically split and join the byte array based on pre-defined number of splits.
        /// Refer <see cref="TemplateEntity"/> for implementation details
        /// </summary>
        public async Task<List<TemplateViewModel>> GetTemplates()
        {
            var query = new TableQuery();
            query.FilterString = new TableFilterBuilder<TemplateEntity>()
                .And(te => te.PartitionKey, CompareOp.EQ, partitionKey);

            var table = await TableUtil.GetTableReference(Tables.Products);
            var azTable = new AzureTable(table);

            var dynamicTemplateEntities = table.ExecuteQuery(query);
            var templateList = new List<TemplateViewModel>();

            foreach(var entity in dynamicTemplateEntities)
            {
                var templateEntity = new TemplateEntity(azTable, entity);
                templateList.Add(TemplateEntity.MapToViewModel(templateEntity));
            }

            return templateList;
        }

        /// <summary>
        /// This implementation uses Wintellect Azure Table library to overcome the limitation of 32KB for string attributes.
        /// The ARM template data is now stored as bytes and Wintellect library is used to automatically split and join the byte array based on pre-defined number of splits.
        /// Refer <see cref="TemplateEntity"/> for implementation details
        /// </summary>
        public async Task<TemplateViewModel> GetTemplate(string templateName)
        {
            var table = await TableUtil.GetTableReference(Tables.Products);
            var azTable = new AzureTable(table);

            var dynamicTemplateEntity =  await TemplateEntity.FindAsync(azTable, partitionKey, templateName);

            var templateViewModel = new TemplateEntity(azTable, dynamicTemplateEntity);
            return TemplateEntity.MapToViewModel(templateViewModel);
        }

        /// <summary>
        /// This implementation uses Wintellect Azure Table library to overcome the limitation of 32KB for string attributes.
        /// The ARM template data is now stored as bytes and Wintellect library is used to automatically split and join the byte array based on pre-defined number of splits.
        /// Refer <see cref="TemplateEntity"/> for implementation details
        /// </summary>
        public async Task<TemplateViewModel> SaveTemplate(TemplateViewModel template)
        {
            template.PartitionKey = partitionKey;
            template.ProductImagePath = await SaveProductImageAsBlob(template.ProductImage);
            template.ProductImage = null; //Null out the base64 data

            var table = await TableUtil.GetTableReference(Tables.Products);
            var azTable = new AzureTable(table);

            var templateEntity = TemplateEntity.MapFromViewModel(template, azTable);
           
            var tableResult = await templateEntity.InsertOrReplaceAsync();
            var templateViewModel = TemplateEntity.MapToViewModel((TemplateEntity)tableResult.Result);

            return templateViewModel;
        }

        private async Task<string> SaveProductImageAsBlob(string productImage)
        {
            string productImagePath = null;
            //First save the template blob if it exists
            if (!string.IsNullOrEmpty(productImage))
            {
                string base64prefix = ";base64,";
                string base64Substring = productImage.Substring(productImage.LastIndexOf(base64prefix) + base64prefix.Length);
                byte[] bytes = Convert.FromBase64String(base64Substring);
                var imagePrefix = "image/";
                int mimeTypeIndexOf = productImage.IndexOf(imagePrefix);
                string contentType = productImage.Substring(mimeTypeIndexOf, productImage.IndexOf(base64prefix) - mimeTypeIndexOf);
                string fileExtension = contentType.Substring(imagePrefix.Length);
                MemoryStream ms = new MemoryStream(bytes);
                string imageUri = await BlobHelpers.SaveToBlobContainer(BlobContainers.ProductImages, ms, fileExtension, contentType, productImagePath);
                productImagePath = imageUri;
            }
            
            return productImagePath;
        }

        /// <summary>
        /// This implementation uses Wintellect Azure Table library to overcome the limitation of 32KB for string attributes.
        /// The ARM template data is now stored as bytes and Wintellect library is used to automatically split and join the byte array based on pre-defined number of splits.
        /// Refer <see cref="TemplateEntity"/> for implementation details
        /// </summary>
        public async Task DeleteTemplate(string rowKey)
        {
            var table = await TableUtil.GetTableReference(Tables.Products);
            var azTable = new AzureTable(table);

            var templateEntity = new TemplateEntity(azTable);
            templateEntity.RowKey = rowKey;
            templateEntity.PartitionKey = partitionKey;
            templateEntity.ETag = "*";

            await templateEntity.DeleteAsync();
        }

        public async Task<List<PolicyLookupPathEntity>> GetPolicyLookupPaths(string subscriptionId)
        {
            var table = await TableUtil.GetTableReference(Tables.PolicyLookupPaths);
            TableQuery<PolicyLookupPathEntity> query = new TableQuery<PolicyLookupPathEntity>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, subscriptionId));
            var result = table.ExecuteQuery(query).ToList();
            return result;
        }

        public async Task<string> GetPolicyLookupPath(string subscriptionId, string policyName)
        {
            var table = await TableUtil.GetTableReference(Tables.PolicyLookupPaths);
            var retrieveOperation = TableOperation.Retrieve<PolicyLookupPathEntity>(subscriptionId, policyName);
            var tableResult = await table.ExecuteAsync(retrieveOperation);
            return (tableResult.Result as PolicyLookupPathEntity)?.PolicyLookupPath;
        }

        public async Task<string> SavePolicyLookupPath(string subscriptionId, string policyName, string lookupPath)
        {
            var lookupPathEntity = new PolicyLookupPathEntity { PartitionKey = subscriptionId, RowKey = policyName, PolicyLookupPath = lookupPath };
            var policyLookupEntity = await TableUtil.SaveTableItem(lookupPathEntity, Tables.PolicyLookupPaths);
            return policyLookupEntity.PolicyLookupPath;
        }

        public async Task DeletePolicyLookupPath(string subscriptionId, string policyName)
        {
            var item = new PolicyLookupPathEntity { PartitionKey = subscriptionId, RowKey = policyName };
            await TableUtil.DeleteTableItem(item, Tables.PolicyLookupPaths);
        }
    }
}