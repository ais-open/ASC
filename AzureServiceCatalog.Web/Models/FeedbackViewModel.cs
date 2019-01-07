using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AzureServiceCatalog.Web.Models
{
    public class FeedbackViewModel
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Subject { get; set; }
        public string Comments { get; set; }
    }
}