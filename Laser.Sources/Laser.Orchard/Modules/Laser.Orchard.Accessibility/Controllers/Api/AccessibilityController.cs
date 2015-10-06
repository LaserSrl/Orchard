using Laser.Orchard.Accessibility.Services;
using Orchard;
using System.Web.Http;

namespace Laser.Orchard.Accessibility.Controllers.Api
{
    public class AccessibilityController : ApiController
    {
        private readonly IOrchardServices _orchardServices;
        private readonly IAccessibilityServices _accessibilityServices;

        public AccessibilityController(IOrchardServices services, IAccessibilityServices acServices)
            : base()
        {
            _orchardServices = services;
            _accessibilityServices = acServices;
        }

        public IHttpActionResult Get()
        {
            string operation = _orchardServices.WorkContext.HttpContext.Request.QueryString.ToString();
            switch (operation)
            {
                case "txt":
                    _accessibilityServices.SetTextOnly();
                    break;
                case "normal":
                    _accessibilityServices.SetNormal();
                    break;
                case "high":
                    _accessibilityServices.SetHighContrast();
                    break;
            }

            return Ok(operation);
        }
    }
}