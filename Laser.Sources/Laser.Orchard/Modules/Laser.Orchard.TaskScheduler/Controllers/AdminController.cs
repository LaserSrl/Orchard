using Orchard;
using Orchard.Localization;
using Orchard.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Laser.Orchard.TaskScheduler.Controllers {
    public class AdminController : Controller {

        private readonly IOrchardServices _orchardServices;

        public Localizer T { get; set; }

        public AdminController(IOrchardServices orchardServices) {
            _orchardServices = orchardServices;
            T = NullLocalizer.Instance;
        }

        public ActionResult Index() {
            if (!_orchardServices.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Not allowed to schedule periodic tasks")))
                return new HttpUnauthorizedResult();

            return null;
        }
    }
}