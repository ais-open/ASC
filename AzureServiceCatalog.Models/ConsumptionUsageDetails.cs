using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureServiceCatalog.Models
{
    public class ConsumptionUsageDetails
    {
        public ConsumptionUsageDetails()
        {
            Value = new List<ConsumptionAggregate>();
        }

        public List<ConsumptionAggregate> Value { get; internal set; }
    }

    public class ConsumptionAggregate
    {
        public ConsumptionAggregate()
        {
            Properties = new Properties();
        }
        public string Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string Tags { get; set; }
        public Properties Properties { get; set; }
    }
    public class Properties
    {
        public string billingAccountId { get; set; }
        public string billingAccountName { get; set; }
        public DateTime billingPeriodStartDate { get; set; }
        public DateTime billingPeriodEndDate { get; set; }
        public string billingProfileId { get; set; }
        public string billingProfileName { get; set; }
        public string accountOwnerId { get; set; }
        public string accountName { get; set; }
        public string subscriptionId { get; set; }
        public string subscriptionName { get; set; }
        public DateTime date { get; set; }
        public string product { get; set; }
        public string partNumber { get; set; }
        public string meterId { get; set; }
        public decimal quantity { get; set; }
        public decimal effectivePrice { get; set; }
        public decimal cost { get; set; }
        public decimal unitPrice { get; set; }
        public string billingCurrency { get; set; }
        public string resourceLocation { get; set; }
        public string consumedService { get; set; }
        public string resourceId { get; set; }
        public string resourceName { get; set; }
        public string invoiceSection { get; set; }
        public string resourceGroup { get; set; }
        public string offerId { get; set; }
        public bool isAzureCreditEligible { get; set; }
        public string publisherType { get; set; }
        public string chargeType { get; set; }
        public string frequency { get; set; }
        public object meterDetails { get; set; }
    }
}
