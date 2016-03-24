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
using Newtonsoft.Json;
using Laser.Orchard.StartupConfig.ViewModels;

namespace Laser.Orchard.Policy.Filters {

    //public class PolicyFilter : FilterProvider, IActionFilter, IResultFilter {
    //    private readonly IOrchardServices _orchardServices;
    //    private readonly ShellSettings _shellSettings;
    //    private readonly HttpRequest _request;
    //    private readonly IUtilsServices _utilsServices;
    //    private string _additionalCacheKey;

    //    public PolicyFilter(ShellSettings shellSettings, IOrchardServices orchardServices, IUtilsServices utilsServices) {
    //        _shellSettings = shellSettings;
    //        _request = HttpContext.Current.Request;
    //        Logger = NullLogger.Instance;
    //        _orchardServices = orchardServices;
    //        _utilsServices = utilsServices;
    //    }

    //    public ILogger Logger;



    //    public void OnActionExecuting(ActionExecutingContext filterContext) {
    //    }

    //    public void OnResultExecuted(ResultExecutedContext filterContext) {
    //    }

    //    public void OnResultExecuting(ResultExecutingContext filterContext) {
    //    }

    //    /// <summary>
    //    /// Called by OutpuCache after the default cache key has been defined
    //    /// </summary>
    //    /// <param name="key">default cache key such as defined in Orchard.OutpuCache</param>
    //    /// <returns>The new cache key</returns>
    //    public System.Text.StringBuilder InflatingCacheKey(System.Text.StringBuilder key) {
    //        ValidateRequestByApiKey(null);
    //        key.Append(_additionalCacheKey);
    //        return key;
    //    }

    //}

}


