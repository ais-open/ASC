using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureServiceCatalog.Models
{
    public class Budget: BaseModel
    {
        public string Code { get; set; }
        public string BlueprintAssignmentId { get; set; }
        public string SubscriptionId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public double Amount { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public BudgetRepeat RepeatType { get; set; }

        public string RepeatTypeString
        {
            get
            {
                return RepeatType.ToString();
            }
        }
    }
}
