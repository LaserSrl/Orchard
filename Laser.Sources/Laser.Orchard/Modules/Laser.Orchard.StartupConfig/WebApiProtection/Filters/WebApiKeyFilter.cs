using Laser.Orchard.StartupConfig.Services;
using Orchard;
using Orchard.Environment.Configuration;
using Orchard.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http.Filters;
using Orchard.ContentManagement;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web.Mvc;

namespace Laser.Orchard.StartupConfig.WebApiProtection.Filters {
    public class WebApiKeyFilter : System.Web.Http.Filters.ActionFilterAttribute {
        private string _additionalCacheKey;
        private bool _protectAlways;

        public WebApiKeyFilter(bool protectAlways) {
            _additionalCacheKey = null;
            _protectAlways = protectAlways;
        }

        public override void OnActionExecuting(System.Web.Http.Controllers.HttpActionContext actionContext) {
            var workContext = actionContext.ControllerContext.GetWorkContext();
            IApiKeyService apiKeyService = null;
            if (workContext.TryResolve<IApiKeyService>(out apiKeyService)) {
                _additionalCacheKey = apiKeyService.ValidateRequestByApiKey(_additionalCacheKey, _protectAlways);
                if ((_additionalCacheKey != null) && (_additionalCacheKey != "AuthorizedApi")) {
                    var result = new JsonResult {
                        Data = "UnauthorizedApi",
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet
                    };
                    actionContext.Response = actionContext.ControllerContext.Request.CreateResponse(HttpStatusCode.Unauthorized, result, "application/json");
                }
                else {
                    base.OnActionExecuting(actionContext);
                }
            }
            else {
                base.OnActionExecuting(actionContext);
            }
        }

        //private void ErrorResult(System.Web.Http.Controllers.HttpActionContext filterContext, string errorData) {
        //    if (filterContext == null) return;
        //    //filterContext.HttpContext.Response.Clear();
        //    //filterContext.HttpContext.Response.TrySkipIisCustomErrors = true;
        //    //filterContext.HttpContext.Response.StatusCode = (int)System.Net.HttpStatusCode.Forbidden;
        //    var response = _utilsServices.GetResponse(ViewModels.ResponseType.UnAuthorized);
        //    response.Data = errorData;
        //    filterContext.Result = new JsonResult {
        //        Data = response,
        //        JsonRequestBehavior = JsonRequestBehavior.AllowGet
        //    };
        //    return;
        //}
    }
}