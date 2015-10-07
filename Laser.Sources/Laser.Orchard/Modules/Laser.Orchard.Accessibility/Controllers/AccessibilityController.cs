using Laser.Orchard.Accessibility.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Net;

namespace Laser.Orchard.Accessibility.Controllers
{
    public class AccessibilityController : Controller
    {
        private IAccessibilityServices _accessibilityServices;

        public AccessibilityController(IAccessibilityServices acServices)
        {
            _accessibilityServices = acServices;
        }
        //
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

            string aux = System.Web.HttpContext.Current.Request.RawUrl;
            aux = aux.Remove(aux.IndexOf("Laser.Orchard.Accessibility/Accessibility"));

            return Redirect(aux);
        }
	}
}