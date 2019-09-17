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

namespace AzureServiceCatalog.Web.Controllers
{
    [RoutePrefix("api/blueprint-assignments")]
    public class BlueprintAssignmentsController : ApiController
    {
        private BlueprintsHelper client = new BlueprintsHelper();

        [Route("")]
        public async Task<IHttpActionResult> Get(string subscriptionId)
        {
            var thisOperationContext = new BaseOperationContext("BlueprintAssignmentsController:Get")
            {
                IpAddress = HttpContext.Current.Request.UserHostAddress,
                UserId = ClaimsPrincipal.Current.SignedInUserName(),
                UserName = ClaimsPrincipal.Current.Identity.Name
            };
            try
            {
                var blueprintAssignments = await this.client.GetBlueprintAssignments(subscriptionId, thisOperationContext);
                var list = new List<object>();
                dynamic updatedBlueprintAssignments = JObject.Parse(blueprintAssignments);
                foreach (var item in updatedBlueprintAssignments.value)
                {
                    var blueprintAssignmentItem = new BlueprintAssignment
                    {
                        Id = item.id,
                        Name = item.name,
                        Type = item.type,
                        Scope = item.properties.scope,
                        Location = item.location,
                        CreatedDate = item.properties.status.timeCreated,
                        LastModifiedDate = item.properties.status.lastModified,
                        BlueprintId = item.properties.blueprintId,
                        ProvisioningState = item.properties.provisioningState,
                        LockMode = item.properties.locks.mode,
                        ManagedIdentity = item.identity.type,
                        ResourceGroups = item.properties.resourceGroups,
                        Parameters = item.properties.parameters,
                    };
                    if (blueprintAssignmentItem.ManagedIdentity == "userAssigned")
                    {
                        blueprintAssignmentItem.UserAssignedIdentities = item.identity.userAssignedIdentities;
                    }
                    var tempArr = blueprintAssignmentItem.BlueprintId.Split('/');
                    var blueprintsIndex = Array.IndexOf(tempArr, "blueprints");
                    var blueprintName = tempArr[blueprintsIndex + 1];
                    blueprintAssignmentItem.BlueprintName = blueprintName;
                    var tempArr1 = blueprintAssignmentItem.BlueprintId.Split('/');
                    var versionsIndex = Array.IndexOf(tempArr, "versions");
                    var blueprintVersion = tempArr[versionsIndex + 1];
                    blueprintAssignmentItem.BlueprintVersion = blueprintVersion;
                    list.Add(blueprintAssignmentItem);
                }
                return this.Ok(list);
            }
            catch (Exception ex)
            {
                TraceHelper.TraceError(thisOperationContext.OperationId, thisOperationContext.OperationName, ex);
                return Content(HttpStatusCode.InternalServerError, JObject.FromObject(ErrorInformation.GetInternalServerErrorInformation()));
            }
            finally
            {
                thisOperationContext.CalculateTimeTaken();
                TraceHelper.TraceOperation(thisOperationContext);
            }
        }

        [Route("{blueprintAssignmentName}")]
        public async Task<IHttpActionResult> Get(string subscriptionId, string blueprintAssignmentName)
        {
            var thisOperationContext = new BaseOperationContext("BlueprintAssignmentsController:Get")
            {
                IpAddress = HttpContext.Current.Request.UserHostAddress,
                UserId = ClaimsPrincipal.Current.SignedInUserName(),
                UserName = ClaimsPrincipal.Current.Identity.Name
            };
            try
            {
                var blueprintAssignment = await this.client.GetBlueprintAssignment(subscriptionId, blueprintAssignmentName, thisOperationContext);
                dynamic item = JObject.Parse(blueprintAssignment);
                BlueprintAssignment blueprintAssignmentItem = null;
                if (item["error"] == null)
                {
                    blueprintAssignmentItem = new BlueprintAssignment
                    {
                        Id = item.id,
                        Name = item.name,
                        Type = item.type,
                        Scope = item.properties.scope,
                        Location = item.location,
                        CreatedDate = item.properties.status.timeCreated,
                        LastModifiedDate = item.properties.status.lastModified,
                        BlueprintId = item.properties.blueprintId,
                        ProvisioningState = item.properties.provisioningState,
                        LockMode = item.properties.locks.mode,
                        ManagedIdentity = item.identity.type,
                        ResourceGroups = item.properties.resourceGroups,
                        Parameters = item.properties.parameters,
                    };
                    if (blueprintAssignmentItem.ManagedIdentity == "userAssigned")
                    {
                        blueprintAssignmentItem.UserAssignedIdentities = item.identity.userAssignedIdentities;
                    }
                    var tempArr = blueprintAssignmentItem.BlueprintId.Split('/');
                    var blueprintsIndex = Array.IndexOf(tempArr, "blueprints");
                    var blueprintName = tempArr[blueprintsIndex + 1];
                    blueprintAssignmentItem.BlueprintName = blueprintName;
                    var tempArr1 = blueprintAssignmentItem.BlueprintId.Split('/');
                    var versionsIndex = Array.IndexOf(tempArr, "versions");
                    var blueprintVersion = tempArr[versionsIndex + 1];
                    blueprintAssignmentItem.BlueprintVersion = blueprintVersion;
                }
                return this.Ok(blueprintAssignmentItem);
            }
            catch (Exception ex)
            {
                TraceHelper.TraceError(thisOperationContext.OperationId, thisOperationContext.OperationName, ex);
                return Content(HttpStatusCode.InternalServerError, JObject.FromObject(ErrorInformation.GetInternalServerErrorInformation()));
            }
            finally
            {
                thisOperationContext.CalculateTimeTaken();
                TraceHelper.TraceOperation(thisOperationContext);
            }
        }

        [Route("{blueprintAssignmentName}")]
        public async Task<IHttpActionResult> Put(string subscriptionId, string blueprintAssignmentName, [FromBody]object blueprintAssignment)
        {
            var thisOperationContext = new BaseOperationContext("BlueprintAssignmentsController:Put");
            try
            {
                dynamic assignment = blueprintAssignment;
                var managedIdentity = assignment.identity.type;
                if (managedIdentity == "SystemAssigned")
                {
                    string objectId = null;
                    string tenantId = ClaimsPrincipal.Current.TenantId();
                    var blueprintSP = await this.client.GetObjectIdOfBlueprintServicePrincipal(subscriptionId, blueprintAssignmentName, thisOperationContext);
                    var responseForBlueprintSP = this.Request.CreateResponse(HttpStatusCode.OK);
                    if (responseForBlueprintSP.IsSuccessStatusCode)
                    {
                        dynamic data = JObject.Parse(blueprintSP);
                        objectId = data.objectId;
                    }
                    RbacHelper rbacClient = new RbacHelper();
                    var json = await rbacClient.GrantRoleForBlueprintAssignment(subscriptionId, "Owner", objectId, thisOperationContext);
                }
                var azureResponse = await this.client.AssignBlueprint(subscriptionId, blueprintAssignmentName, blueprintAssignment, thisOperationContext);
                var responseMsg = this.Request.CreateResponse(HttpStatusCode.OK);
                responseMsg.Content = azureResponse.ToStringContent();
                IHttpActionResult response = ResponseMessage(responseMsg);
                return response;
            }
            catch(Exception ex)
            {
                TraceHelper.TraceError(thisOperationContext.OperationId, thisOperationContext.OperationName, ex);
                return Content(HttpStatusCode.InternalServerError, JObject.FromObject(ErrorInformation.GetInternalServerErrorInformation()));
            }
            finally
            {
                thisOperationContext.CalculateTimeTaken();
                TraceHelper.TraceOperation(thisOperationContext);
            }
        }
    }
}