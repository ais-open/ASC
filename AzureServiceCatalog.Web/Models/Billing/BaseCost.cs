using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AzureServiceCatalog.Web.Models.Billing
{
    public class BaseCost : ICost
    {
        public double BillableQuantity { get; set; }
        public ResourceUsage ResourceUsage { get; protected set; }
        public Meter Meter { get; protected set; }

        protected virtual void CalculateBillableQuantity()
        {
            BillableQuantity = ResourceUsage.Quantity - Meter.IncludedQuantity;
            if (BillableQuantity < 0)
            {
                BillableQuantity = 0;
            }
        }

        public virtual double CalculateCosts(ResourceUsage resourceUsage, Meter meter)
        {
            ResourceUsage = resourceUsage;
            Meter = meter;
            CalculateBillableQuantity();
            return CalculateCosts();
        }

        protected virtual double CalculateCosts()
        {
            throw new NotImplementedException();
        }
    }
}