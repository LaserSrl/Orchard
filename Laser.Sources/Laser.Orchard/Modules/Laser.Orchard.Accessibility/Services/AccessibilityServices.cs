using Orchard;
using Orchard.Environment.Configuration;
using Orchard.Logging;
using System;
using System.Web;

namespace Laser.Orchard.Accessibility.Services
{
    public class AccessibilityServices : IAccessibilityServices
    {
        private readonly IOrchardServices _orchardServices;
        private readonly ShellSettings _shellSettings;
        public ILogger Logger { get; set; }

        public AccessibilityServices(IOrchardServices orchardServices, ShellSettings shellSettings)
        {
            _orchardServices = orchardServices;
            _shellSettings = shellSettings;
            Logger = NullLogger.Instance;
        }

        private void setCookie(string cookieValue)
        {
            // calcola il path corretto per il cookie
            string path = "/"; // path del cookie
            string tenantPath = _shellSettings.RequestUrlPrefix ?? "";
            string operation = _orchardServices.WorkContext.HttpContext.Request.QueryString.ToString();
            string appPath = _orchardServices.WorkContext.HttpContext.Request.ApplicationPath;
            //Logger.Warning("tenantPath=" + tenantPath);
            //Logger.Warning("operation=" + operation);
            //Logger.Warning("appPath=" + appPath);

            if (tenantPath == "")
            {
                path = appPath;
            }
            else
            {
                appPath = (appPath.EndsWith("/")) ? appPath : appPath + "/";
                path = appPath + tenantPath;
            }
            //Logger.Warning("path=" + path);

            // setta il cookie
            HttpCookie cook = new HttpCookie(Utils.AccessibilityCookieName);
            cook.Path = path;
            cook.Value = cookieValue;
            if (cookieValue == "")
            {
                // elimina il cookie
                cook.Expires = DateTime.UtcNow.AddMonths(-1);
            }
            else
            {
                cook.Expires = DateTime.UtcNow.AddMonths(1);
            }
            _orchardServices.WorkContext.HttpContext.Response.SetCookie(cook);
        }

        public void SetTextOnly()
        {
            setCookie(Utils.AccessibilityTextOnly);
        }

        public void SetNormal()
        {
            setCookie(Utils.AccessibilityNormal);
        }

        public void SetHighContrast()
        {
            setCookie(Utils.AccessibilityHighContrast);
        }
    }
}