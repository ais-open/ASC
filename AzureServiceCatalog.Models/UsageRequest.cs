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
    public class UsageRequest: BaseModel
    {
        public DateTime StartDate { get; set; }
        public string Subscriptions { get; set; }
        public string RessourceGroups { get; set; }
    }
}