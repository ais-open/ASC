using Microsoft.Azure.Management.Resources.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using AzureServiceCatalog.Models;
using Microsoft.WindowsAzure.Storage.Table;

namespace AzureServiceCatalog.Helpers.BudgetHelper
{
    public class BudgetRepository : IRepository<Budget>
    {
        private IAzureTableRepository<BudgetTableEntity> budgetDataRep;

        public BudgetRepository()
        {
            budgetDataRep = new AzureTableRepository<BudgetTableEntity>(BudgetTableEntity.TableName);
        }

        public async Task<IEnumerable<Budget>> GetList()
        {
            var entities = await budgetDataRep.GetList(new TableQuery<BudgetTableEntity>());
            IList<Budget> budgets = new List<Budget>();
            entities.ForEach(x => budgets.Add(Mapper.Map(x)));
            return budgets;
        }

        public async Task<Budget> GetSingle(string subscriptionId, string code)
        {
            var entity = await budgetDataRep.GetSingle(subscriptionId, code);
            if (entity == null) return null;
            return Mapper.Map(entity);
        }

        public async Task<IEnumerable<Budget>> GetListByDepCode(string code)
        {
            var entities = await budgetDataRep.GetList(new TableQuery<BudgetTableEntity>()
                .Where(TableQuery.GenerateFilterCondition("OrgEntityCode", QueryComparisons.Equal, code)));

            IList<Budget> budgets = new List<Budget>();
            entities.ForEach(x => budgets.Add(Mapper.Map(x)));
            return budgets;
        }

        public async Task Add(Budget model)
        {
            await budgetDataRep.Add(Mapper.Map(model));
        }

        public async Task Update(Budget model)
        {
            var entity = await budgetDataRep.GetSingle(model.SubscriptionId, model.Code);

            if (entity != null)
            {
                entity.BlueprintAssignmentId = model.BlueprintAssignmentId;
                entity.Name = model.Name;
                entity.Description = model.Description;
                entity.Amount = model.Amount;
                entity.StartDate = model.StartDate;
                entity.EndDate = model.EndDate;
                entity.RepeatType = (int)model.RepeatType;

                await budgetDataRep.Update(entity);
            }
        }

        public async Task Delete(Budget model)
        {
            var entity = await budgetDataRep.GetSingle(model.SubscriptionId, model.Code);

            if (entity != null)
            {
                await budgetDataRep.Delete(entity.SubscriptionId, entity.RowKey);
            }
        }


    }
}