using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AzureServiceCatalog.Web.Models
{
    public enum SecurityGroupType
    {
        Invalid = 0,
        CanCreate = 1,
        CanDepoy = 2,
        CanAdmin = 3
    }
}