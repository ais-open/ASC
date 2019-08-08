using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AzureServiceCatalog.Models
{
    /// <summary>
    /// Represents the Table names available for the Service Catalog in Azure Table Storage.
    /// </summary>
    public static class Tables
    {
        public const string Organizations = "Organizations";
        public const string Subscription = "Subscription";
        public const string Products = "Products";
        public const string PerUserTokenCache = "PerUserTokenCache";
        public const string PolicyLookupPaths = "PolicyLookupPaths";
    }
}