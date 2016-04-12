using System;
using System.Web;
using Orchard;
using Orchard.Localization.Services;
using Orchard.UI.Admin;

namespace Laser.Orchard.CulturePicker.Services {
    public class CookieCultureSelector : ICultureSelector {
        private readonly IOrchardServices _orchardServices;
        public const string CultureCookieName = "cultureData";
        public const string CurrentCultureFieldName = "currentCulture";
        public const int SelectorPriority = -2; //priority is higher than SiteCultureSelector priority (-5), But lower than ContentCultureSelector (0) 

        private CultureSelectorResult _evaluatedResult;
        private bool _isEvaluated;

        #region ICultureSelector Members
        public CookieCultureSelector(IOrchardServices orchardServices) {
            _orchardServices = orchardServices;
            
        }

        public CultureSelectorResult GetCulture(HttpContextBase context) {
            if (!_isEvaluated) {
                _isEvaluated = true;
                _evaluatedResult = EvaluateResult(context);
            }

            return _evaluatedResult;
        }

        #endregion

        #region Helpers

        private CultureSelectorResult EvaluateResult(HttpContextBase context) {

            if (context == null || context.Request == null || context.Request.Cookies == null) {
                return null;
            }
            if (AdminFilter.IsApplied(context.Request.RequestContext)) { // I am in admin context so I have to use defualt site culture
                return new CultureSelectorResult { Priority = SelectorPriority, CultureName = _orchardServices.WorkContext.CurrentSite.SiteCulture };
            }

            HttpCookie cultureCookie = context.Request.Cookies[context.Request.AnonymousID + CultureCookieName];
            if (cultureCookie == null) {
                return null;
            }

            if (context.Request.Url != null && context.Request.Url.IsDefaultPort) {
                // '.' prefix means, that cookie will be shared to sub-domains
                cultureCookie.Domain = "." + context.Request.Url.Host;
            }

            string currentCultureName = cultureCookie[CurrentCultureFieldName];
            return String.IsNullOrEmpty(currentCultureName) ? null : new CultureSelectorResult {Priority = SelectorPriority, CultureName = currentCultureName};
        }

        #endregion
    }
}