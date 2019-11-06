using AzureServiceCatalog.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureServiceCatalog.Helpers.BudgetHelper
{
    public static class Mapper
    {

        public static BudgetTableEntity Map(Budget model)
        {
            return new BudgetTableEntity
            {
                RowKey = model.Code,
                BlueprintAssignmentId = model.BlueprintAssignmentId,
                SubscriptionId = model.SubscriptionId,
                Name = model.Name,
                Description = model.Description,
                Amount = model.Amount,
                StartDate = model.StartDate,
                EndDate = model.EndDate,
                RepeatType = (int)model.RepeatType
            };
        }

        public static Budget Map(BudgetTableEntity entity)
        {
            return new Budget
            {
                Code = entity.RowKey,
                BlueprintAssignmentId = entity.BlueprintAssignmentId,
                SubscriptionId = entity.SubscriptionId,
                Name = entity.Name,
                Description = entity.Description,
                Amount = entity.Amount,
                StartDate = entity.StartDate,
                EndDate = entity.EndDate,
                RepeatType = (BudgetRepeat)entity.RepeatType
            };
        }

    }
}
