using AzureServiceCatalog.Models.Billing;
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

namespace AzureServiceCatalog.Helpers.BudgetHelper
{
    public interface IRepository<T> where T : BaseModel
    {
        Task<IEnumerable<T>> GetList();
        Task<T> GetSingle(string code);
        Task<IEnumerable<T>> GetListByDepCode(string code);
        Task Add(T model);
        Task Update(T model);
        Task Delete(T model);
    }
}