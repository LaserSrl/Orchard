using Laser.Orchard.Accessibility.Services;
using System.Web.Mvc;
using Orchard;

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

            // Se non arrivo dalla navigazione sulla pagina
            if (_orchardServices.WorkContext.HttpContext.Request.UrlReferrer == null)
                //throw new System.Web.HttpException(404, "Not found");
                return HttpNotFound();
            else {
                // calcola l'url di ritorno: è la pagina in cui è stato richiamato il controller
                string returnUrl = _orchardServices.WorkContext.HttpContext.Request.UrlReferrer.AbsoluteUri;

                return Redirect(returnUrl);
            }
        }
	}
}