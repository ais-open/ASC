using AzureServiceCatalog.Models;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace AzureServiceCatalog.Helpers.BudgetHelper
{
    public class UsageHelper
    {
        public async Task<UsageResponse> GetUsageData(UsageRequest requestParams)
        {
            IAzureTableRepository<UsageTableEntity> usageDataRep = new AzureTableRepository<UsageTableEntity>(UsageTableEntity.TableName);

            UsageResponse response = new UsageResponse() { Value = new List<Usage>() };
            DateTime dateToFetch = requestParams.StartDate;
            string pKey = String.Empty;

            List<string> subscriptions = new List<string>();
            if (!String.IsNullOrEmpty(requestParams.Subscriptions))
            {
                subscriptions.AddRange(requestParams.Subscriptions.Split(',').Select(e => e.ToLower()));
            }

            List<string> resourceGroups = new List<string>();
            if (!String.IsNullOrEmpty(requestParams.RessourceGroups))
            {
                resourceGroups.AddRange(requestParams.RessourceGroups.Split(',').Select(e => e.ToLower()));
            }

            for (int i = 0; i <= 12; i++)
            {
                dateToFetch = dateToFetch.AddMonths(i);
                if (dateToFetch > DateTime.Now) break;

                pKey = dateToFetch.ToString("MMM", CultureInfo.InvariantCulture) + dateToFetch.Year;
                var entities = await usageDataRep.GetList(new TableQuery<UsageTableEntity>()
                .Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, pKey)));

                if (entities.Count > 0)
                {
                    if (subscriptions.Count > 0)
                    {
                        entities = entities
                            .Where(e => subscriptions.Contains(e.SubscriptionId.ToLower()))
                            .ToList();
                    }

                    if (resourceGroups.Count > 0)
                    {
                        entities = entities
                            .Where(e => resourceGroups.Contains(e.ResourceGroup.ToLower()))
                            .ToList();
                    }

                    if (entities.Count > 0)
                    {
                        response.Value.AddRange(entities
                        .GroupBy(e => e.Service)
                        .Select(g => new Usage
                        {
                            Month = dateToFetch.Month,
                            Year = dateToFetch.Year,
                            ServiceName = g.First().Service,
                            Cost = g.Sum(c => c.Cost),
                        }));
                    }
                }
            }

            return response;
        }
    }
}
