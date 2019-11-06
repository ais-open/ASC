using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureServiceCatalog.Helpers.BudgetRepositories
{
    public class OrgTableEntity : TableEntity
    {
        public static string TableName = "Org";

        public OrgTableEntity()
        {
            PartitionKey = TableName;
        }

        public string Name { get; set; }
        public string Description { get; set; }
        public string ParentCode { get; set; }
    }
}
