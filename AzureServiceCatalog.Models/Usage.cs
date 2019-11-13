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
    public class Usage : BaseModel
    {
        public int Month { get; set; }
        public int Year { get; set; }
        public string ServiceName { get; set; }
        public double Cost { get; set; }
    }
}