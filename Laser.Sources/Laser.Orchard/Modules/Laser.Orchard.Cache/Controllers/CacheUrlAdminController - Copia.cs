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
        private const string contentType = "CacheURL";
        private readonly ICacheAliasServices _cacheAliasServices;
        private IRepository<CacheUrlRecord> _cacheUrlRepository;

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
        public ActionResult Index(int? page, int? pageSize, SearchVM search) {
            if (!_orchardServices.Authorizer.Authorize(Permissions.UrlCache)) {
                return new HttpUnauthorizedResult();
            }
            //var records = new IEnumerable<CacheUrlRecord>();
            IEnumerable<CacheUrlRecord> records;
            int totItems = 0;
            Pager pager = new Pager(_orchardServices.WorkContext.CurrentSite, page, pageSize);
            var expression = search.Expression?.ToLower();
            if (string.IsNullOrEmpty(expression)) {
                records = _cacheUrlRepository.Fetch(x => true).OrderBy(x => x.Priority);
            }
            else {
                var searchrecords = _cacheUrlRepository.Fetch(x => x.CacheToken.Contains(expression) || x.CacheURL.Contains(expression)).OrderBy(x => x.Priority).ToList();
                totItems = searchrecords.Count();
                records = searchrecords.Skip(pager.GetStartIndex()).Take(pager.PageSize);
            }

            records = _cacheUrlRepository.Fetch(x => true).OrderBy(x => x.Priority).ToList();
            var pagerShape = _orchardServices.New.Pager(pager).TotalItemCount(totItems);
            return View("Index", new SearchIndexVM(records, search, pagerShape, null));
            //return View("Index", new CacheUrlVM { Cached = records });
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
                        var old = _cacheUrlRepository.Get(x.Id);
                        _cacheUrlRepository.Delete(old);
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