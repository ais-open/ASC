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
    [RoutePrefix("api/budgets")]
    public class BudgetController : ApiController
    {
        private IRepository<Budget> rep;

        public BudgetController()
        {
            rep = new BudgetRepository();
        }

        [Route("")]
        public async Task<IHttpActionResult> Get(string subscriptionId)
        {
            var thisOperationContext = new BaseOperationContext("BudgetController:Get")
            {
                IpAddress = HttpContext.Current.Request.UserHostAddress,
                UserId = ClaimsPrincipal.Current.SignedInUserName(),
                UserName = ClaimsPrincipal.Current.Identity.Name
            };
            try
            {
                var list = await rep.GetList();
                return this.Ok(list);
            }
            catch (Exception ex)
            {
                TraceHelper.TraceError(thisOperationContext.OperationId, thisOperationContext.OperationName, ex);
                return Content(HttpStatusCode.InternalServerError, JObject.FromObject(ErrorInformation.GetInternalServerErrorInformation(thisOperationContext.OperationId, thisOperationContext.Timestamp)));
            }
            finally
            {
                thisOperationContext.CalculateTimeTaken();
                TraceHelper.TraceOperation(thisOperationContext);
            }
        }

        [Route("{code}")]
        public async Task<IHttpActionResult> Get(string subscriptionId, string code)
        {
            var thisOperationContext = new BaseOperationContext("BudgetController:Get")
            {
                IpAddress = HttpContext.Current.Request.UserHostAddress,
                UserId = ClaimsPrincipal.Current.SignedInUserName(),
                UserName = ClaimsPrincipal.Current.Identity.Name
            };
            try
            {
                var budget = await rep.GetSingle(code);
                return this.Ok(budget);
            }
            catch (Exception ex)
            {
                TraceHelper.TraceError(thisOperationContext.OperationId, thisOperationContext.OperationName, ex);
                return Content(HttpStatusCode.InternalServerError, JObject.FromObject(ErrorInformation.GetInternalServerErrorInformation(thisOperationContext.OperationId, thisOperationContext.Timestamp)));
            }
            finally
            {
                thisOperationContext.CalculateTimeTaken();
                TraceHelper.TraceOperation(thisOperationContext);
            }
        }

        [Route("")]
        public async Task<IHttpActionResult> Post([FromBody]object budget)
        {
            var thisOperationContext = new BaseOperationContext("BudgetController:Post")
            {
                IpAddress = HttpContext.Current.Request.UserHostAddress,
                UserId = ClaimsPrincipal.Current.SignedInUserName(),
                UserName = ClaimsPrincipal.Current.Identity.Name
            };
            try
            {
                dynamic item = budget;
                var budgetModel = new Budget
                {
                    Code = Guid.NewGuid().ToString(),
                    BlueprintAssignmentId = item.blueprintAssignmentId,
                    SubscriptionId = item.subscriptionId,
                    Name = item.name,
                    Description = item.description,
                    Amount = item.amount,
                    StartDate = item.startDate,
                    EndDate = item.endDate,
                    RepeatType = (BudgetRepeat)item.repeatType
                };
                var existingModel = await rep.GetSingle(budgetModel.BlueprintAssignmentId);
                if (existingModel != null) return BadRequest();

                await rep.Add(budgetModel);
                return this.Ok(budgetModel.BlueprintAssignmentId);
            }
            catch (Exception ex)
            {
                TraceHelper.TraceError(thisOperationContext.OperationId, thisOperationContext.OperationName, ex);
                return Content(HttpStatusCode.InternalServerError, JObject.FromObject(ErrorInformation.GetInternalServerErrorInformation(thisOperationContext.OperationId, thisOperationContext.Timestamp)));
            }
            finally
            {
                thisOperationContext.CalculateTimeTaken();
                TraceHelper.TraceOperation(thisOperationContext);
            }
        }

        [Route("")]
        public async Task<IHttpActionResult> Put([FromBody]object updatedBudget)
        {
            var thisOperationContext = new BaseOperationContext("BudgetController:Put")
            {
                IpAddress = HttpContext.Current.Request.UserHostAddress,
                UserId = ClaimsPrincipal.Current.SignedInUserName(),
                UserName = ClaimsPrincipal.Current.Identity.Name
            };
            try
            {
                dynamic item = updatedBudget;
                var budgetModel = new Budget
                {
                    Code = item.code,
                    BlueprintAssignmentId = item.blueprintAssignmentId,
                    SubscriptionId = item.subscriptionId,
                    Name = item.name,
                    Description = item.description,
                    Amount = item.amount,
                    StartDate = item.startDate,
                    EndDate = item.endDate,
                    RepeatType = item.repeatType
                };
                var existingModel = await rep.GetSingle(budgetModel.Code);
                if (existingModel == null)
                {
                    ErrorInformation errorInformation = new ErrorInformation();
                    errorInformation.Code = "InvalidRequest";
                    errorInformation.Message = "Request body is invalid.";
                    return Content(HttpStatusCode.BadRequest, JObject.FromObject(errorInformation));
                } else
                {
                    await rep.Update(budgetModel);
                    return this.Ok(budgetModel.BlueprintAssignmentId);
                }
            }
            catch (Exception ex)
            {
                TraceHelper.TraceError(thisOperationContext.OperationId, thisOperationContext.OperationName, ex);
                return Content(HttpStatusCode.InternalServerError, JObject.FromObject(ErrorInformation.GetInternalServerErrorInformation(thisOperationContext.OperationId, thisOperationContext.Timestamp)));
            }
            finally
            {
                thisOperationContext.CalculateTimeTaken();
                TraceHelper.TraceOperation(thisOperationContext);
            }
        }

        [Route("{blueprintAssignmentId}")]
        public async Task<IHttpActionResult> Delete(string blueprintAssignmentId)
        {
            var thisOperationContext = new BaseOperationContext("BudgetController:Delete")
            {
                IpAddress = HttpContext.Current.Request.UserHostAddress,
                UserId = ClaimsPrincipal.Current.SignedInUserName(),
                UserName = ClaimsPrincipal.Current.Identity.Name
            };
            try
            {
                var existingModel = await rep.GetSingle(blueprintAssignmentId);
                if (existingModel == null) return BadRequest();

                await rep.Delete(existingModel);
                return this.Ok();
            }
            catch (Exception ex)
            {
                TraceHelper.TraceError(thisOperationContext.OperationId, thisOperationContext.OperationName, ex);
                return Content(HttpStatusCode.InternalServerError, JObject.FromObject(ErrorInformation.GetInternalServerErrorInformation(thisOperationContext.OperationId, thisOperationContext.Timestamp)));
            }
            finally
            {
                thisOperationContext.CalculateTimeTaken();
                TraceHelper.TraceOperation(thisOperationContext);
            }
        }

    }
}