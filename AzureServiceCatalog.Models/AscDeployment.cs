using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureServiceCatalog.Models
{
    public class AscDeployment
    {
        public string ResourceGroupName { get; set; }
        public string DeploymentName { get; set; }
        public string Parameters { get; set; }
        public string Template { get; set; }
        public string TemplateName { get; set; }
        public string SubscriptionId { get; set; }
    }
}
