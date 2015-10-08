using Laser.Orchard.Accessibility.Services;
using System.Linq;
using System.Web.Mvc;
using Orchard;
using Orchard.OutputCache.Models;

namespace Laser.Orchard.Accessibility.Controllers
{
    public class AccessibilityController : Controller
    {
        private IOrchardServices _orchardServices;
        private IAccessibilityServices _accessibilityServices;

        public AccessibilityController(IOrchardServices services, IAccessibilityServices acServices)
        {
            _orchardServices = services;
            _accessibilityServices = acServices;

            // verifica le impostazioni dell'output cache diOrchard e, se necessario, li aggiorna aggiungendo il cookie dell'accessibilità
            var cacheSettings = _orchardServices.WorkContext.CurrentSite.ContentItem.Parts.OfType<CacheSettingsPart>().First();
            if (cacheSettings != null)
            {
                string vary = cacheSettings.VaryCookieStringParameters ?? "";
                if (vary.Contains(Utils.AccessibilityCookieName) == false)
                {
                    if (string.IsNullOrWhiteSpace(vary))
                    {
                        vary = Utils.AccessibilityCookieName;
                    }
                    else
                    {
                        vary += "," + Utils.AccessibilityCookieName;
                    }
                    cacheSettings.VaryCookieStringParameters = vary;
                }
            }
        }

        // GET: /Accessibility/
        public ActionResult Index()
        {
            string operation = System.Web.HttpContext.Current.Request.QueryString.ToString();

            switch (operation)
            {
                case "txt":
                    _accessibilityServices.SetTextOnly();
                    break;
                case "high":
                    _accessibilityServices.SetHighContrast();
                    break;
                case "normal":
                    _accessibilityServices.SetNormal();
                    break;
            }

            // calcola l'url di ritorno: è la pagina in cui è stato richiamato il controller
            string returnUrl = _orchardServices.WorkContext.HttpContext.Request.UrlReferrer.AbsoluteUri;

            return Redirect(returnUrl);
        }
	}
}