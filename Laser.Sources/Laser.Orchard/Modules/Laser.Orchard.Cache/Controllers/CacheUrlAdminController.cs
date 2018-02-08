using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Laser.Orchard.Cache.Models;
using Laser.Orchard.Cache.Services;
using Laser.Orchard.Cache.ViewModels;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Data;
using Orchard.UI.Admin;
using Orchard.UI.Navigation;

namespace Laser.Orchard.Cache.Controllers {

    public class CacheURLAdminController : Controller {
        private readonly IOrchardServices _orchardServices;
        private readonly IContentManager _contentManager;
        private readonly ICacheAliasServices _cacheAliasServices;
        private readonly IRepository<CacheUrlRecord> _cacheUrlRepository;

        public CacheURLAdminController(
            IContentManager contentManager,
            IRepository<CacheUrlRecord> cacheUrlRepository,
            IOrchardServices orchardServices,
            ICacheAliasServices cacheAliasServices
            ) {
            _orchardServices = orchardServices;
            _contentManager = contentManager;
            _cacheUrlRepository = cacheUrlRepository;
            _cacheAliasServices = cacheAliasServices;
        }

        [HttpGet]
        [Admin]
        public ActionResult Index() {
            if (!_orchardServices.Authorizer.Authorize(Permissions.UrlCache)) {
                return new HttpUnauthorizedResult();
            }
            var records = _cacheUrlRepository.Table.OrderByDescending(x => x.Priority).ToList();
            return View("Index", new CacheUrlVM { Cached = records });
        }

        [HttpPost]
        [Admin]
        public ActionResult Index(CacheUrlVM vm) {
            if (!_orchardServices.Authorizer.Authorize(Permissions.UrlCache)) {
                return new HttpUnauthorizedResult();
            }
            foreach (var x in vm.Cached) {
                if (x.Id == 0) {
                    if (!string.IsNullOrEmpty(x.CacheURL)) {
                        x.CacheURL = x.CacheURL.ToLower();
                        _cacheUrlRepository.Create(x);
                    }
                }
                else {
                    if (string.IsNullOrEmpty(x.CacheURL)) {
                        _cacheUrlRepository.Delete(_cacheUrlRepository.Get(r=>r.Id==x.Id));
                    }
                    else {
                        x.CacheURL = x.CacheURL.ToLower();
                        _cacheUrlRepository.Update(x);
                    }
                }
                _cacheUrlRepository.Flush();
            }
            _cacheAliasServices.RefreshCachedRouteConfig(_cacheUrlRepository);
            return RedirectToAction("Index");
        }


    }
}