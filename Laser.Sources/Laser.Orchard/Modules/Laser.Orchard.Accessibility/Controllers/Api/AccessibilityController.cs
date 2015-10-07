using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using Laser.Orchard.Accessibility.Services;

namespace Laser.Orchard.Accessibility.Controllers.Api
{
    public class AccessibilityController : ApiController
    {
        private IAccessibilityServices _accessibilityServices;

        public AccessibilityController(IAccessibilityServices acServices)
        {
            _accessibilityServices = acServices;
        }

        public IHttpActionResult Get()
        {
            string operation = HttpContext.Current.Request.QueryString.ToString();

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

            return Ok(operation);
        }
    }
}