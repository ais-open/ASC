using AzureServiceCatalog.Web.Models;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using AzureServiceCatalog.Helpers;
using AzureServiceCatalog.Models;
using System.Web;
using System.Security.Claims;
using System;
using AzureServiceCatalog.Helpers.BudgetHelper;

namespace AzureServiceCatalog.Web.Controllers
{
    [RoutePrefix("api/usage")]
    public class UsageController : ApiController
    {

        [Route("")]
        public async Task<IHttpActionResult> Post([FromBody]UsageRequest requestParams)
        {
            ChartHelper chartHelper = new ChartHelper();
            BudgetChartData chartData = await chartHelper.GetBudgetChartData(requestParams);
            return this.Ok(chartData);
        }
    }
}