using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureServiceCatalog.Models
{
    public class Users
    {
        public List<User> value { get; set; }
    }

    public class User
    {
        public string objectType { get; set; }
        public string objectId { get; set; }
        public string displayName { get; set; }
        public string userPrincipalName { get; set; }
        public string userType { get; set; }
    }
}