using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Web;

namespace AzureServiceCatalog.Web.Models
{
    public class TemplateViewModel
    {
        public TemplateViewModel() {
            RowKey = Guid.NewGuid().ToString();
        }

        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public string TemplateData { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string ProductImagePath { get; set; } //Image URI
        public string ProductImage { get; set; } //Image Base64String, only used during upload
        public double ProductPrice { get; set; }
        public bool IsPublished { get; set; }
    }
}