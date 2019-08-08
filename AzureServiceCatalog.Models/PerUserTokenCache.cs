using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using Microsoft.WindowsAzure.Storage.Table;
using System.Diagnostics.CodeAnalysis;

namespace AzureServiceCatalog.Models
{
    public class PerUserTokenCache : TableEntity
    {
        public PerUserTokenCache()
        {
            PartitionKey = "Azure Service Catalog";
            RowKey = Guid.NewGuid().ToString();
        }

        [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "web", Justification = "Just temporary")]
        public string webUserUniqueId { get; set; }

        [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "cache", Justification = "Just temporary")]
        [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
        [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "web", Justification = "Just temporary")]
        public byte[] cacheBits { get; set; }

        public DateTime LastWrite { get; set; }
    }
}
