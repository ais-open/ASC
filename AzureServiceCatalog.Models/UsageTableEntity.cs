using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Web;
using Wintellect.Azure.Storage.Table;

namespace AzureServiceCatalog.Models
{
    public class UsageTableEntity : TableEntity
    {
        public static string TableName = "Usage";

        public UsageTableEntity()
        {
        }

        public UsageTableEntity(string pKey, string rKey)
        {
            PartitionKey = pKey;
            RowKey = rKey;
        }

        public DateTime Date { get; set; }
        public string Service { get; set; }
        public double Cost { get; set; }
        public string SubscriptionId { get; set; }
        public string ResourceGroup { get; set; }
        public string ResourceName { get; set; }
        public string ResourceId { get; set; }
    }
}