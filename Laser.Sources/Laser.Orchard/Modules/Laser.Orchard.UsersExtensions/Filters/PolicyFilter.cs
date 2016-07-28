using Laser.Orchard.Commons.Services;
using Laser.Orchard.Policy.Models;
using Laser.Orchard.Policy.Services;
using Laser.Orchard.StartupConfig.Services;
using Laser.Orchard.StartupConfig.ViewModels;
using Laser.Orchard.UsersExtensions.Services;
using Newtonsoft.Json.Linq;
using Orchard;
using Orchard.Logging;
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

        public ILogger Logger { get; set; }

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

                        // logga la richiesta per un certo periodo di tempo
                        //if (DateTime.Today.ToString("yyyyMMdd").CompareTo("20160723") < 0) {
                        //    string url = "";
                        //    string controller = "";
                        //    string action = "";

                        //    try {
                        //        if (filterContext != null) {
                        //            if (filterContext.HttpContext != null) {
                        //                if (filterContext.HttpContext.Request != null) {
                        //                    url = filterContext.HttpContext.Request.RawUrl;
                        //                }
                        //                else {
                        //                    url = "No URL available: Request is null.";
                        //                }
                        //            }
                        //            else {
                        //                url = "No URL available: HttpContext is null.";
                        //            }
                        //            if (filterContext.ActionDescriptor != null) {
                        //                action = filterContext.ActionDescriptor.ActionName;
                        //                if (filterContext.ActionDescriptor.ControllerDescriptor != null) {
                        //                    controller = filterContext.ActionDescriptor.ControllerDescriptor.ControllerName;
                        //                }
                        //                else {
                        //                    controller = "No controller available: ControlerDescriptor is null.";
                        //                }
                        //            }
                        //            else {
                        //                controller = "No controller available: ActionDescriptor is null.";
                        //            }
                        //        }
                        //        else {
                        //            url = "No URL available: filterContext is null.";
                        //        }
                        //        Logger.Error(string.Format("UsersExtensions Policy Filter - Request: {0}, Controller: {1}, Action: {2}.", url, controller, action));
                        //    }
                        //    catch {
                        //        // ignora volutamente qualsiasi errore
                        //    }
                        //}

                        if (filterContext.Controller.GetType().FullName == "Laser.Orchard.WebServices.Controllers.JsonController") {
                            string data = PoliciesLMNVSerialization(neededPolicies.Where(w => missingPolicies.Any(a => a == w.Id)));

                            filterContext.Result = new ContentResult { Content = data, ContentType = "application/json" };
                        }
                        else if (filterContext.Controller.GetType().FullName == "Laser.Orchard.WebServices.Controllers.WebApiController") {
                            string data = PoliciesPureJsonSerialization(neededPolicies.Where(w => missingPolicies.Any(a => a == w.Id)));

                            filterContext.Result = new ContentResult { Content = data, ContentType = "application/json" };
                        }
                        else {
                            string outputFormat = _workContext.GetContext().HttpContext.Request.Headers["OutputFormat"];

                            if (String.Equals(outputFormat, "LMNV", StringComparison.OrdinalIgnoreCase)) {
                                string data = PoliciesLMNVSerialization(neededPolicies.Where(w => missingPolicies.Any(a => a == w.Id)));
                                Response response = _utilsServices.GetResponse(ResponseType.MissingPolicies, "", Newtonsoft.Json.JsonConvert.DeserializeObject(data));

                                filterContext.Result = new ContentResult { Content = Newtonsoft.Json.JsonConvert.SerializeObject(response), ContentType = "application/json" };
                            }
                            else if (String.Equals(outputFormat, "PureJson", StringComparison.OrdinalIgnoreCase)) {
                                string data = PoliciesPureJsonSerialization(neededPolicies.Where(w => missingPolicies.Any(a => a == w.Id)));
                                Response response = _utilsServices.GetResponse(ResponseType.MissingPolicies, "", Newtonsoft.Json.JsonConvert.DeserializeObject(data));

                                filterContext.Result = new ContentResult { Content = Newtonsoft.Json.JsonConvert.SerializeObject(response), ContentType = "application/json" };
                            }
                            else {
                                var returnType = ((ReflectedActionDescriptor)filterContext.ActionDescriptor).MethodInfo.ReturnType;

                                if (returnType == typeof(JsonResult)) {
                                    string data = PoliciesPureJsonSerialization(neededPolicies.Where(w => missingPolicies.Any(a => a == w.Id)));
                                    Response response = _utilsServices.GetResponse(ResponseType.MissingPolicies, "", Newtonsoft.Json.JsonConvert.DeserializeObject(data));

                                    filterContext.Result = new ContentResult { Content = Newtonsoft.Json.JsonConvert.SerializeObject(response), ContentType = "application/json" };
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
                }
            }

            return;
        }

        public void OnActionExecuted(ActionExecutedContext filterContext) { }

        private string PoliciesLMNVSerialization(IEnumerable<PolicyTextInfoPart> policies) {
            ObjectDumper dumper;
            StringBuilder sb = new StringBuilder();
            XElement dump = null;

            var realFormat = false;
            var minified = false;
            bool.TryParse(_workContext.GetContext().HttpContext.Request["realformat"], out realFormat);
            bool.TryParse(_workContext.GetContext().HttpContext.Request["minified"], out minified);

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

            return sb.ToString();
        }

        private string PoliciesPureJsonSerialization(IEnumerable<PolicyTextInfoPart> policies) {
            JObject json = new JObject();
            var resultArray = new JArray();

            foreach (var pendingPolicy in policies) {
                resultArray.Add(new JObject(_contentSerializationServices.SerializeContentItem(pendingPolicy.ContentItem, 0)));
            }

            json.Add("PendingPolicies", resultArray);

            return json.ToString(Newtonsoft.Json.Formatting.None);
        }
    }
}