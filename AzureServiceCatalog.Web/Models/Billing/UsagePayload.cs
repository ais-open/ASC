using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AzureServiceCatalog.Web.Models.Billing
{
    public class UsagePayload
    {
        public UsagePayload()
        {
            Value = new List<UsageAggregate>();
        }

        public List<UsageAggregate> Value { get; internal set; }
    }
}