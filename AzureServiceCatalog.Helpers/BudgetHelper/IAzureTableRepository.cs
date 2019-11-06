using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AzureServiceCatalog.Helpers.BudgetHelper
{
    public interface IAzureTableRepository<T> where T:ITableEntity, new()
    {
        Task<List<T>> GetList(TableQuery<T> query);
        Task<T> GetSingle(string rowKey);
        Task Add(T entity);
        Task Update(T entity);
        Task Delete(string rowKey);
    }
}
