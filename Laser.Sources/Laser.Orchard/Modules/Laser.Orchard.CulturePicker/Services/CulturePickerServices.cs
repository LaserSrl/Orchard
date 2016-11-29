using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard;

namespace Laser.Orchard.CulturePicker.Services {
    public class CulturePickerServices : ICulturePickerServices {
        private readonly IOrchardServices _orchardServices;
       private readonly IWorkContextAccessor _workContextAccessor;
        //public CulturePickerServices(IWorkContextAccessor workContextAccessor) {
        //    _workContextAccessor = workContextAccessor;
        //}
       public CulturePickerServices(IOrchardServices orchardServices, IWorkContextAccessor workContextAccessor) {
            _orchardServices = orchardServices;
            _workContextAccessor = workContextAccessor;
        }
        public void SaveCultureCookie(string cultureName, HttpContextBase context) {
            HttpRequestBase request = context.Request;

            var cultureCookie = new HttpCookie(CookieCultureSelector.CultureCookieName);
            cultureCookie.Values.Add(CookieCultureSelector.CurrentCultureFieldName, cultureName);
            cultureCookie.Expires = DateTime.UtcNow.AddMonths(1);

            //setting up domain for cookie allows to share it to sub-domains as well
            //if non-default port is used, we consider it as a testing environment without sub-domains
            if (request.Url != null && request.Url.IsDefaultPort) {
                // '.' prefix means, that cookie will be shared to sub-domains
                cultureCookie.Domain = "." + request.Url.Host;
            }

            context.Response.Cookies.Add(cultureCookie);
        }

    }
}