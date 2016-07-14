﻿using Laser.Orchard.Commons.Services;
using Laser.Orchard.Policy.Models;
using Laser.Orchard.Policy.Services;
using Laser.Orchard.StartupConfig.Services;
using Laser.Orchard.UsersExtensions.Services;
using Newtonsoft.Json.Linq;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Mvc;
using Orchard.Mvc.Filters;
using Orchard.UI.Admin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Xml.Linq;

namespace Laser.Orchard.UsersExtensions.Filters {
    public class PolicyFilter : FilterProvider, IActionFilter {

        private readonly IContentSerializationServices _contentSerializationServices;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IPolicyServices _policyServices;
        private readonly IUsersExtensionsServices _userExtensionServices;
        private readonly IUtilsServices _utilsServices;
        private readonly IWorkContextAccessor _workContext;

        private string[] allowedControllers;

        public PolicyFilter(IContentSerializationServices contentSerializationServices,
                            IHttpContextAccessor httpContextAccessor,
                            IPolicyServices policyServices,
                            IUsersExtensionsServices userExtensionServices,
                            IUtilsServices utilsServices,
                            IWorkContextAccessor workContext) {
            _contentSerializationServices = contentSerializationServices;
            _httpContextAccessor = httpContextAccessor;
            _policyServices = policyServices;
            _userExtensionServices = userExtensionServices;
            _utilsServices = utilsServices;
            _workContext = workContext;

            allowedControllers = new string[] { "Laser.Orchard.Policy.Controllers.PoliciesController", "Orchard.Users.Controllers.AccountController", "Laser.Orchard.OpenAuthentication.Controllers.AccountController", "Laser.Orchard.Mobile.Controllers.CountPanelController", "Laser.Orchard.MailCommunication.Controllers.CountPanelController" };
        }

        public void OnActionExecuting(ActionExecutingContext filterContext) {

            if (_workContext.GetContext().CurrentUser != null && !allowedControllers.Contains(filterContext.Controller.GetType().FullName) && !AdminFilter.IsApplied(filterContext.RequestContext)) {
                var language = _workContext.GetContext().CurrentCulture;
                IEnumerable<PolicyTextInfoPart> neededPolicies = _userExtensionServices.GetUserLinkedPolicies(language);

                if (neededPolicies.Count() > 0) {
                    // estraggo le policy obbligatorie accettate + facoltative che hanno una data di risposta
                    var userPolicies = _policyServices.GetPoliciesForUserOrSession(false, language).Policies.Where(w => w.Accepted || (w.AnswerDate > DateTime.MinValue && !w.PolicyText.UserHaveToAccept)).Select(s => s.PolicyTextId).ToList();
                    var missingPolicies = neededPolicies.Select(s => s.Id).ToList().Where(w => !userPolicies.Any(a => a == w));

                    if (missingPolicies.Count() > 0) {
                        if (filterContext.Controller.GetType().FullName == "Laser.Orchard.WebServices.Controllers.JsonController") {
                            ObjectDumper dumper;
                            StringBuilder sb = new StringBuilder();
                            XElement dump = null;

                            var realFormat = false;
                            var minified = false; 
                            bool.TryParse(_workContext.GetContext().HttpContext.Request["realformat"], out realFormat);
                            bool.TryParse(_workContext.GetContext().HttpContext.Request["minified"], out minified);



                            var policies = neededPolicies.Where(w => missingPolicies.Any(a => a == w.Id));

                            sb.Insert(0, "{");
                            sb.AppendFormat("\"n\": \"{0}\"", "Model");
                            sb.AppendFormat(", \"v\": \"{0}\"", "VirtualContent");
                            sb.Append(", \"m\": [{");
                            sb.AppendFormat("\"n\": \"{0}\"", "VirtualId");
                            sb.AppendFormat(", \"v\": \"{0}\"", "0");
                            sb.Append("}]");

                            sb.Append(", \"l\":[");

                            int i = 0;
                            sb.Append("{");
                            sb.AppendFormat("\"n\": \"{0}\"", "PendingPolicies");
                            sb.AppendFormat(", \"v\": \"{0}\"", "ContentItem[]");
                            sb.Append(", \"m\": [");

                            foreach (var item in policies) {
                                if (i > 0) {
                                    sb.Append(",");
                                }
                                sb.Append("{");
                                dumper = new ObjectDumper(10);
                                dump = dumper.Dump(item.ContentItem, String.Format("[{0}]", i));
                                JsonConverter.ConvertToJSon(dump, sb, minified, realFormat);
                                sb.Append("}");
                                i++;
                            }
                            sb.Append("]");
                            sb.Append("}");

                            sb.Append("]");
                            sb.Append("}");

                            filterContext.Result = new ContentResult { Content = sb.ToString(), ContentType = "application/json" };
                        }
                        else if (filterContext.Controller.GetType().FullName == "Laser.Orchard.WebServices.Controllers.WebApiController") {
                            JObject json = new JObject();
                            var resultArray = new JArray();

                            var policies = neededPolicies.Where(w => missingPolicies.Any(a => a == w.Id));

                            foreach (var pendingPolicy in policies) {
                                resultArray.Add(new JObject(_contentSerializationServices.SerializeContentItem(pendingPolicy.ContentItem, 0)));
                            }

                            json.Add("PendingPolicies", resultArray);

                            filterContext.Result = new ContentResult { Content = json.ToString(Newtonsoft.Json.Formatting.None), ContentType = "application/json" };
                        }
                        else {
                            var encodedAssociatedPolicies = Convert.ToBase64String(Encoding.UTF8.GetBytes(string.Join(",", missingPolicies)));

                            UrlHelper urlHelper = new UrlHelper(_httpContextAccessor.Current().Request.RequestContext);
                            var url = urlHelper.Action("Index", "Policies", new { area = "Laser.Orchard.Policy", lang = language, policies = encodedAssociatedPolicies, returnUrl = _httpContextAccessor.Current().Request.RawUrl });

                            filterContext.Result = new RedirectResult(url);
                        }
                    }
                }
            }

            return;
        }

        public void OnActionExecuted(ActionExecutedContext filterContext) { }
    }
}