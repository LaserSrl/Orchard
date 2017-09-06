using Laser.Orchard.SEO.Services;
using Orchard;
using Orchard.DisplayManagement;
using Orchard.Environment.Extensions;
using Orchard.Settings;
using Orchard.UI.Admin;
using Orchard.UI.Navigation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Laser.Orchard.SEO.Controllers {
    [Admin]
    [OrchardFeature("Laser.Orchard.Redirects")]
    public class RedirectsAdminController : Controller {
        private readonly IRedirectService _redirectService;
        private readonly IOrchardServices _orchardServices;
        private readonly ISiteService _siteService;

        private dynamic Shape { get; set; }

        public RedirectsAdminController(
            IRedirectService redirectService,
            IOrchardServices orchardServices,
            ISiteService siteService,
            IShapeFactory shapeFactory) {

            _redirectService = redirectService;
            _orchardServices = orchardServices;
            _siteService = siteService;
            Shape = shapeFactory;
        }

        [HttpGet]
        public ActionResult Index(PagerParameters pagerParameters) {
            var pager = new Pager(_siteService.GetSiteSettings(), pagerParameters);
            var pagerShape = _orchardServices.New.Pager(pager).TotalItemCount(_redirectService.GetRedirectsTotalCount());
            var items = _redirectService.GetRedirects(pager.GetStartIndex(), pager.PageSize);

            dynamic viewModel = Shape.ViewModel()
                .Redirects(items)
                .Pager(pagerShape);
            return View((object)viewModel);
        }
    }
}