using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureServiceCatalog.Web.Models.Billing
{
    public interface ICost
    {
        double CalculateCosts(ResourceUsage resourceUsage, Meter meter);
    }
}
