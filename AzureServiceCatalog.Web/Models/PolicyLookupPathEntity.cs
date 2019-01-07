using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AzureServiceCatalog.Web.Models
{
    public class PolicyLookupPathEntity : TableEntity
    {
        public string PolicyLookupPath { get; set; }
    }
}