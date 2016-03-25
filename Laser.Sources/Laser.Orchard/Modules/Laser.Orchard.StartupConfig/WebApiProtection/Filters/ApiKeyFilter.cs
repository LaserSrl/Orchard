using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Orchard;
using Orchard.Caching;
using Orchard.Environment.Configuration;
using Orchard.Environment.Extensions;
using Orchard.Logging;
using Orchard.Mvc.Filters;
using Orchard.OutputCache.Filters;
using Orchard.Security;
using Orchard.Utility.Extensions;
using Orchard.ContentManagement;
using Laser.Orchard.StartupConfig.WebApiProtection.Models;
using Laser.Orchard.StartupConfig.Services;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;

namespace Laser.Orchard.StartupConfig.WebApiProtection.Filters {

    /// <summary>
    /// A fini di test è possibile passare la ApiKey in QueryString nel seguente formato: OZVV5TpP4U6wJthaCORZEQ,10/03/2016T10.00.00+2
    /// Se ApiKey viene passato in QueryString non viene applicata la logica di cifratura.
    /// Se ApiKey viene passato in QueryString insieme al parametro clear=false invece, viene applicata la logica di cifratura.
    /// </summary>
    [OrchardFeature("Laser.Orchard.StartupConfig.WebApiProtection")]
    public class ApiKeyFilter : FilterProvider, IActionFilter, IResultFilter, ICachingEventHandler  {
        private readonly IApiKeyService _apiKeyService;
        private readonly HttpRequest _request;
        private string _additionalCacheKey;

        public ApiKeyFilter(IApiKeyService apiKeyService) {
            _request = HttpContext.Current.Request;
            Logger = NullLogger.Instance;
            _apiKeyService = apiKeyService;
        }

        public ILogger Logger;

        public void OnActionExecuted(ActionExecutedContext filterContext) {
        }

        public void OnActionExecuting(ActionExecutingContext filterContext) {
            _additionalCacheKey = _apiKeyService.ValidateRequestByApiKey(_additionalCacheKey);
            if ((_additionalCacheKey != null)&&(_additionalCacheKey != "AuthorizedApi")) {
                filterContext.Result = new EmptyResult();

                // il codice seguente non impedisce l'esecuzione della action
                //HttpContext.Current.Response.Clear();
                //HttpContext.Current.Response.StatusCode = 401;
                //HttpContext.Current.Response.Write("Error");
                //HttpContext.Current.Response.End();
            }
        }

        public void OnResultExecuted(ResultExecutedContext filterContext) {
        }

        public void OnResultExecuting(ResultExecutingContext filterContext) {
        }

        /// <summary>
        /// Called by OutpuCache after the default cache key has been defined
        /// </summary>
        /// <param name="key">default cache key such as defined in Orchard.OutpuCache</param>
        /// <returns>The new cache key</returns>
        public System.Text.StringBuilder InflatingCacheKey(System.Text.StringBuilder key) {
            _additionalCacheKey = _apiKeyService.ValidateRequestByApiKey(_additionalCacheKey);
            key.Append(_additionalCacheKey);
            return key;
        }
    }
}


