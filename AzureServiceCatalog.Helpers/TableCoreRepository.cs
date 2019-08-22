using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using AzureServiceCatalog.Models;

namespace AzureServiceCatalog.Helpers
{
    /// <summary>
    /// This repository handles data access for the "Core" tables. The Core tables are the ones that live in the actual
    /// Service Catalog's storage account, as opposed to the "non-Core" tables which are stored in the user's own storage account.
    /// </summary>
    public class TableCoreRepository
    {
        private const string partitionKey = "Azure Service Catalog";

        internal void SavePerUserTokenCaches(PerUserTokenCache cache, BaseOperationContext parentOperationContext)
        {
            var thisOperationContext = new BaseOperationContext(parentOperationContext, "TableCoreRepository:SavePerUserTokenCaches");
            try
            {
                TableHelper.SaveCoreTableItemSync<PerUserTokenCache>(cache, Tables.PerUserTokenCache, thisOperationContext);
            }
            finally
            {
                thisOperationContext.CalculateTimeTaken();
                TraceHelper.TraceOperation(thisOperationContext);
            }
        }

        internal List<PerUserTokenCache> GetPerUserTokenCacheListById(string User, BaseOperationContext parentOperationContext)
        {
            var thisOperationContext = new BaseOperationContext(parentOperationContext, "TableCoreRepository:GetPerUserTokenCacheListById");
            try
            {
                var table = TableHelper.GetCoreTableReference(Tables.PerUserTokenCache, thisOperationContext);
                var query = new TableQuery<PerUserTokenCache>().Where(TableQuery.GenerateFilterCondition("webUserUniqueId", QueryComparisons.Equal, User));
                var result = table.ExecuteQuery(query);
                if (result != null)
                {
                    return result.ToList();
                }
                return null;
            }
            finally
            {
                thisOperationContext.CalculateTimeTaken();
                TraceHelper.TraceOperation(thisOperationContext);
            }
        }

        internal async Task SaveSubscription(Subscription subscription, BaseOperationContext parentOperationContext)
        {
            var thisOperationContext = new BaseOperationContext(parentOperationContext, "TableCoreRepository:SaveSubscription");
            try
            {
                await TableHelper.SaveCoreTableItem(subscription, Tables.Subscription, thisOperationContext);
            }
            finally
            {
                thisOperationContext.CalculateTimeTaken();
                TraceHelper.TraceOperation(thisOperationContext);
            }
        }

        public async Task SaveOrganization(Organization organization, BaseOperationContext parentOperationContext)
        {
            var thisOperationContext = new BaseOperationContext(parentOperationContext, "TableCoreRepository:SaveOrganization");
            try
            {
                organization.VerifiedDomain = organization.VerifiedDomain.ToLower();
                if (!organization.EnrolledDate.HasValue)
                {
                    organization.EnrolledDate = DateTime.Now;
                }
                await TableHelper.SaveCoreTableItem(organization, Tables.Organizations, thisOperationContext);
            }
            finally
            {
                thisOperationContext.CalculateTimeTaken();
                TraceHelper.TraceOperation(thisOperationContext);
            }
        }

        public async Task DeleteOrganization(Organization organization, BaseOperationContext parentOperationContext)
        {
            var thisOperationContext = new BaseOperationContext(parentOperationContext, "TableCoreRepository:DeleteOrganization");
            try
            {
                List<Subscription> relatedSubscriptions = GetSubscriptionListByOrgId(organization.Id, thisOperationContext);
                await TableHelper.DeleteCoreTableItem(organization, Tables.Organizations, thisOperationContext);
                //Delete all subscriptions associated with the organization
                foreach (Subscription relatedSubscription in relatedSubscriptions)
                {
                    await DeleteSubscription(relatedSubscription, thisOperationContext);
                }
            }
            finally
            {
                thisOperationContext.CalculateTimeTaken();
                TraceHelper.TraceOperation(thisOperationContext);
            }
        }

        public List<Subscription> GetSubscriptionListByOrgId(string orgID, BaseOperationContext parentOperationContext)
        {
            var thisOperationContext = new BaseOperationContext(parentOperationContext, "TableCoreRepository:GetSubscriptionListByOrgId");
            try
            {
                var table = TableHelper.GetCoreTableReference(typeof(Subscription).Name, thisOperationContext);
                var query = new TableQuery<Subscription>().Where(TableQuery.GenerateFilterCondition("OrganizationId", QueryComparisons.Equal, orgID));
                var result = table.ExecuteQuery(query);
                if (result != null)
                {
                    return result.ToList();
                }
                return null;
            }
            finally
            {
                thisOperationContext.CalculateTimeTaken();
                TraceHelper.TraceOperation(thisOperationContext);
            }
        }

        public Subscription GetSubscription(string subscriptionID, BaseOperationContext parentOperationContext)
        {
            var thisOperationContext = new BaseOperationContext(parentOperationContext, "TableCoreRepository:GetSubscription");
            try
            {
                var table = TableHelper.GetCoreTableReference(typeof(Subscription).Name, thisOperationContext);
                var retrieveOperation = TableOperation.Retrieve<Subscription>(partitionKey, subscriptionID);
                var tableResult = table.Execute(retrieveOperation);
                return (Subscription)tableResult.Result;
            }
            finally
            {
                thisOperationContext.CalculateTimeTaken();
                TraceHelper.TraceOperation(thisOperationContext);
            }
        }

        public async Task DeleteSubscription(Subscription subscription, BaseOperationContext parentOperationContext)
        {
            var thisOperationContext = new BaseOperationContext(parentOperationContext, "TableCoreRepository:DeleteSubscription");
            try
            {
                await TableHelper.DeleteCoreTableItem(subscription, Tables.Subscription, thisOperationContext);
            }
            finally
            {
                thisOperationContext.CalculateTimeTaken();
                TraceHelper.TraceOperation(thisOperationContext);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        public async Task<Organization> GetOrganization(string organizationId, BaseOperationContext parentOperationContext)
        {
            var thisOperationContext = new BaseOperationContext(parentOperationContext, "TableCoreRepository:GetOrganization");
            try
            {
                var table = TableHelper.GetCoreTableReference(Tables.Organizations, thisOperationContext);
                var retrieveOperation = TableOperation.Retrieve<Organization>(partitionKey, organizationId);
                var tableResult = await table.ExecuteAsync(retrieveOperation);
                return (Organization)tableResult.Result;
            }
            finally
            {
                thisOperationContext.CalculateTimeTaken();
                TraceHelper.TraceOperation(thisOperationContext);
            }

        }

        public Organization GetOrganizationByDomain(string domain, BaseOperationContext parentOperationContext)
        {
            var thisOperationContext = new BaseOperationContext(parentOperationContext, "TableCoreRepository:GetOrganizationByDomain");
            try
            {
                var table = TableHelper.GetCoreTableReference(Tables.Organizations, thisOperationContext);
                var filter = TableQuery.GenerateFilterCondition("VerifiedDomain", QueryComparisons.Equal, domain);
                var query = new TableQuery<Organization>().Where(filter);
                var result = table.ExecuteQuery(query);
                if (result != null)
                {
                    return result.SingleOrDefault();
                }
                return null;
            }
            finally
            {
                thisOperationContext.CalculateTimeTaken();
                TraceHelper.TraceOperation(thisOperationContext);
            }
        }

        public Organization GetOrganizationSync(string organizationId, BaseOperationContext parentOperationContext)
        {
            var thisOperationContext = new BaseOperationContext(parentOperationContext, "TableCoreRepository:GetOrganizationSync");
            try
            {
                var table = TableHelper.GetCoreTableReference(Tables.Organizations, thisOperationContext);
                var retrieveOperation = TableOperation.Retrieve<Organization>(partitionKey, organizationId);
                var tableResult = table.Execute(retrieveOperation);
                return (Organization)tableResult.Result;
            }
            finally
            {
                thisOperationContext.CalculateTimeTaken();
                TraceHelper.TraceOperation(thisOperationContext);
            }
        }

        internal void ClearAllPerUserTokenCache()
        {
            ///// GetTableReference(typeof(PerUserTokenCache).Name).Delete();
        }

        public List<Subscription> GetEnrolledSubscriptionListByOrgId(string orgID, BaseOperationContext parentOperationContext)
        {
            var thisOperationContext = new BaseOperationContext(parentOperationContext, "TableCoreRepository:GetEnrolledSubscriptionListByOrgId");
            try
            {
                var table = TableHelper.GetCoreTableReference(Tables.Subscription, thisOperationContext);

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
            finally
            {
                thisOperationContext.CalculateTimeTaken();
                TraceHelper.TraceOperation(thisOperationContext);
            }
        }
    }
}