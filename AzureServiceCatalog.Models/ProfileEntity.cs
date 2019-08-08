using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Web;

namespace AzureServiceCatalog.Models
{
    public class ProfileEntity : TableEntity
    {
        public string Template { get; set; }
        public string ProfileData { get; set; }
    }

}