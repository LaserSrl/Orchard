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

namespace Laser.Orchard.StartupConfig.WebApiProtection.Filters {
    public class WebApiKeyFilter : ActionFilterAttribute {
        private string _additionalCacheKey;
        private bool _protectAlways;

        public WebApiKeyFilter(bool protectAlways) {
            _additionalCacheKey = null;
            _protectAlways = protectAlways;
        }

        public override void OnActionExecuting(System.Web.Http.Controllers.HttpActionContext actionContext) {
            var workContext = actionContext.ControllerContext.GetWorkContext();
            var apiKeyService = workContext.Resolve<IApiKeyService>();
            _additionalCacheKey = apiKeyService.ValidateRequestByApiKey(_additionalCacheKey, _protectAlways);
            if ((_additionalCacheKey != null) && (_additionalCacheKey != "AuthorizedApi")) {
                actionContext.Response = actionContext.ControllerContext.Request.CreateResponse(HttpStatusCode.Unauthorized);
            }
            else {
                base.OnActionExecuting(actionContext);
            }
        }
    }
}