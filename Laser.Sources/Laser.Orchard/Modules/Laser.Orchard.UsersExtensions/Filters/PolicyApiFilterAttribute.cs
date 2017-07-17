using Laser.Orchard.Commons.Attributes;
using Laser.Orchard.Policy.Models;
using Laser.Orchard.Policy.Services;
using Laser.Orchard.UsersExtensions.Services;
using Newtonsoft.Json.Linq;
using Orchard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace Laser.Orchard.UsersExtensions.Filters {
    /// <summary>
    /// This action filter prevents an action from being executed if the calling user
    /// has not accepted the required policies
    /// </summary>
    public class PolicyApiFilterAttribute : ActionFilterAttribute {
        
        public PolicyApiFilterAttribute() { }

        public override void OnActionExecuting(HttpActionContext actionContext) {
            bool isAdminService = actionContext
                .ActionDescriptor
                .GetCustomAttributes<AdminServiceAttribute>(false)
                .Any();

            var _workContext = actionContext.ControllerContext.GetWorkContext();
            var _userExtensionServices = _workContext.Resolve<IUsersExtensionsServices>();
            var _policyService = _workContext.Resolve<IPolicyServices>();
            var currentUser = _workContext.CurrentUser;
            
            if (currentUser != null &&
                _userExtensionServices != null &&
                _policyService != null &&
                !isAdminService) {

                var language = _workContext.CurrentCulture;
                IEnumerable<PolicyTextInfoPart> neededPolicies = _userExtensionServices
                    .GetUserLinkedPolicies(language);

                if (neededPolicies.Any()) {
                    var userPolicyIds = _policyService
                        .GetPoliciesForUserOrSession(false, language)
                        .Policies
                        .Where(po =>
                            po.Accepted ||
                            (po.AnswerDate > DateTime.MinValue && !po.PolicyText.UserHaveToAccept))
                        .Select(po => po.PolicyTextId);
                    var missingPolicyIds = neededPolicies
                        .Select(po => po.Id)
                        .Except(userPolicyIds);

                    if (missingPolicyIds.Any()) {
                        string data = _policyService
                            .PoliciesLMNVSerialization(neededPolicies
                                .Where(po => missingPolicyIds.Contains(po.Id)));
                        actionContext.Response = actionContext
                            .ControllerContext
                            .Request
                            .CreateResponse(HttpStatusCode.OK, JObject.Parse(data), "application/json");
                    }

                }
            }
        }
    }
}