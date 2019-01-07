using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace AzureServiceCatalog.Web.Models
{
    /// <summary>
    /// This repository handles data access for the "Core" tables. The Core tables are the ones that live in the actual
    /// Service Catalog's storage account, as opposed to the "non-Core" tables which are stored in the user's own storage account.
    /// </summary>
    public class TableCoreRepository
    {
        private const string partitionKey = "Azure Service Catalog";

        internal void SavePerUserTokenCaches(PerUserTokenCache cache)
        {
            TableUtil.SaveCoreTableItemSync<PerUserTokenCache>(cache, Tables.PerUserTokenCache);
        }

        internal List<PerUserTokenCache> GetPerUserTokenCacheListById(string User)
        {
            var table = TableUtil.GetCoreTableReference(Tables.PerUserTokenCache);
            var query = new TableQuery<PerUserTokenCache>().Where(TableQuery.GenerateFilterCondition("webUserUniqueId", QueryComparisons.Equal, User));
            var result = table.ExecuteQuery(query);
            if (result != null)
            {
                return result.ToList();
            }
            return null;
        }

        internal async Task SaveSubscription(Subscription subscription)
        {
            await TableUtil.SaveCoreTableItem(subscription, Tables.Subscription);
        }

        internal async Task SaveOrganization(Organization organization)
        {
            organization.VerifiedDomain = organization.VerifiedDomain.ToLower();
            if (!organization.EnrolledDate.HasValue)
            {
                organization.EnrolledDate = DateTime.Now;
            }
            await TableUtil.SaveCoreTableItem(organization, Tables.Organizations);
        }

        internal async Task DeleteOrganization(Organization organization)
        {
            List<Subscription> relatedSubscriptions = GetSubscriptionListByOrgId(organization.Id);
            await TableUtil.DeleteCoreTableItem(organization, Tables.Organizations);
            //Delete all subscriptions associated with the organization
            foreach (Subscription relatedSubscription in relatedSubscriptions)
            {
                await DeleteSubscription(relatedSubscription);
            }
        }

        internal List<Subscription> GetSubscriptionListByOrgId(string orgID)
        {
            var table = TableUtil.GetCoreTableReference(typeof(Subscription).Name);
            var query = new TableQuery<Subscription>().Where(TableQuery.GenerateFilterCondition("OrganizationId", QueryComparisons.Equal, orgID));
            var result = table.ExecuteQuery(query);
            if (result != null)
            {
                return result.ToList();
            }
            return null;
        }

        internal Subscription GetSubscription(string subscriptionID)
        {
            var table = TableUtil.GetCoreTableReference(typeof(Subscription).Name);
            var retrieveOperation = TableOperation.Retrieve<Subscription>(partitionKey, subscriptionID);
            var tableResult = table.Execute(retrieveOperation);
            return (Subscription)tableResult.Result;
        }
        internal async Task DeleteSubscription(Subscription subscription)
        {
            await TableUtil.DeleteCoreTableItem(subscription, Tables.Subscription);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        internal async Task<Organization> GetOrganization(string organizationId)
        {
            var table = TableUtil.GetCoreTableReference(Tables.Organizations);
            var retrieveOperation = TableOperation.Retrieve<Organization>(partitionKey, organizationId);
            var tableResult = await table.ExecuteAsync(retrieveOperation);
            return (Organization)tableResult.Result;
        }

        internal Organization GetOrganizationByDomain(string domain)
        {
            var table = TableUtil.GetCoreTableReference(Tables.Organizations);
            var filter = TableQuery.GenerateFilterCondition("VerifiedDomain", QueryComparisons.Equal, domain);
            var query = new TableQuery<Organization>().Where(filter);
            var result = table.ExecuteQuery(query);
            if (result != null)
            {
                return result.SingleOrDefault();
            }
            return null;
        }

        internal Organization GetOrganizationSync(string organizationId)
        {
            var table = TableUtil.GetCoreTableReference(Tables.Organizations);
            var retrieveOperation = TableOperation.Retrieve<Organization>(partitionKey, organizationId);
            var tableResult = table.Execute(retrieveOperation);
            return (Organization)tableResult.Result;
        }

        internal void ClearAllPerUserTokenCache()
        {
            ///// GetTableReference(typeof(PerUserTokenCache).Name).Delete();
        }

        internal List<Subscription> GetEnrolledSubscriptionListByOrgId(string orgID)
        {
            var table = TableUtil.GetCoreTableReference(Tables.Subscription);

            string orgFilter = TableQuery.GenerateFilterCondition("OrganizationId", QueryComparisons.Equal, orgID);
            string isEnrolledFilter = TableQuery.GenerateFilterConditionForBool("IsEnrolled", QueryComparisons.Equal, true);
            string combinedFilters = TableQuery.CombineFilters(orgFilter, TableOperators.And, isEnrolledFilter);

            var query = new TableQuery<Subscription>().Where(combinedFilters);
            var result = table.ExecuteQuery(query);
            if (result != null)
            {
                return result.ToList();
            }
            return null;
        }
    }
}