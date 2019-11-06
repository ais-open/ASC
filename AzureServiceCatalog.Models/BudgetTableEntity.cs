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
    public class BudgetTableEntity : TableEntity
    {
        public static string TableName = "Budget";

        public BudgetTableEntity()
        {
            PartitionKey = TableName;
        }
        public string BlueprintAssignmentId { get; set; }
        public string SubscriptionId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public double Amount { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int RepeatType { get; set; }
        public string Subscriptions { get; set; }
        public string ResourceGroups { get; set; }
        public string OrgEntityCode { get; set; }
    }
}