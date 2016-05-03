﻿using System;
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
using Newtonsoft.Json.Linq;
using Laser.Orchard.Policy.Models;

namespace Laser.Orchard.Policy.Filters {

    public class PolicyFilter : FilterProvider, IActionFilter, IResultFilter, ICachingEventHandler {
        private readonly ICommonsServices _commonServices;
        private readonly IContentSerializationServices _contentSerializationServices;
        private IList<IContent> pendingPolicies;

        public PolicyFilter(ICommonsServices commonServices,
            IContentSerializationServices contentSerializationServices) {
            _commonServices = commonServices;
            _contentSerializationServices = contentSerializationServices;
        }

        public ILogger Logger;



        public void OnActionExecuting(ActionExecutingContext filterContext) {

            SetPendingPolicies();
            if (pendingPolicies != null && pendingPolicies.Count() > 0) {
                JObject json;
                json = new JObject();
                var resultArray = new JArray();
                foreach (var pendingPolicy in pendingPolicies) {
                    resultArray.Add(new JObject(_contentSerializationServices.SerializeContentItem((ContentItem)pendingPolicy, 0)));
                }
                json.Add("PendingPolicies", resultArray);
                _contentSerializationServices.NormalizeSingleProperty(json);
                filterContext.Result = new ContentResult { Content = json.ToString(Newtonsoft.Json.Formatting.None), ContentType = "application/json" };
                //return GetJson(content, page, pageSize);
            }
        }

        public void OnResultExecuted(ResultExecutedContext filterContext) {
        }

        public void OnResultExecuting(ResultExecutingContext filterContext) {
        }

        public void OnActionExecuted(ActionExecutedContext filterContext) {
        }

        public StringBuilder InflatingCacheKey(StringBuilder key) {

            SetPendingPolicies();

            if (pendingPolicies != null && pendingPolicies.Count() > 0)
                key.Append("pendingpolicies=" + String.Join("_", pendingPolicies.Select(s => s.Id)) + ";");

            return key;
        }

        private void SetPendingPolicies() {
            if (pendingPolicies != null) return;
            string areaName = HttpContext.Current.Request.RequestContext.RouteData.Values["area"].ToString();
            string controllerName = HttpContext.Current.Request.RequestContext.RouteData.Values["controller"].ToString();
            string actionName = HttpContext.Current.Request.RequestContext.RouteData.Values["action"].ToString();
            if (areaName.Equals("Laser.Orchard.WebServices", StringComparison.InvariantCultureIgnoreCase) &&
                controllerName.Equals("WebApi", StringComparison.InvariantCultureIgnoreCase) &&
                actionName.Equals("display", StringComparison.InvariantCultureIgnoreCase)) {
                string alias = HttpContext.Current.Request.Params["alias"].ToString();

                JObject json;
                var content = _commonServices.GetContentByAlias(alias);
                //_maxLevel = maxLevel;
                var policy = content.As<Laser.Orchard.Policy.Models.PolicyPart>();
                if (policy != null && (policy.HasPendingPolicies ?? false)) { // Se l'oggetto ha delle pending policies allora devo serivre la lista delle pending policies
                    pendingPolicies = policy.PendingPolicies;
                } else {
                    pendingPolicies = new List<IContent>();
                }
            }


        }
    }

}


