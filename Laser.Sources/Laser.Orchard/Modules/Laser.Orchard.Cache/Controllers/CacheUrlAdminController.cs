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
        public ActionResult Index(int? page, int? pageSize, SearchVM search) {
            if (!_orchardServices.Authorizer.Authorize(Permissions.UrlCache)) {
                return new HttpUnauthorizedResult();
            }
            IEnumerable<CacheUrlRecord> records;
            IEnumerable<CacheUrlRecord> searchrecords;
            int totItems = 0;
            Pager pager = new Pager(_orchardServices.WorkContext.CurrentSite, page, pageSize);
            var expression = search.Expression?.ToLower();
            searchrecords = string.IsNullOrEmpty(expression) ? (IEnumerable<CacheUrlRecord>)_cacheUrlRepository.Fetch(x => true).OrderBy(x => x.CacheURL) : (IEnumerable<CacheUrlRecord>)_cacheUrlRepository.Fetch(x => x.CacheToken.Contains(expression) || x.CacheURL.Contains(expression)).OrderBy(x => x.CacheURL);
            totItems = searchrecords.Count();
            records = searchrecords.Skip(pager.GetStartIndex()).Take(pager.PageSize);
            var pagerShape = _orchardServices.New.Pager(pager).TotalItemCount(totItems);
            return View("Index", new SearchIndexVM(records, search, pagerShape, null));
        }

        [HttpGet]
        [Admin]
        public ActionResult Edit(int id) {
            var model = new CacheUrlRecord();
            if (id != 0)
                model = _cacheUrlRepository.Get(x => x.Id == id);
            return View("Edit", model);
        }

        [HttpPost]
        [Admin]
        public ActionResult Edit(CacheUrlRecord record) {
            if (record.Id == 0) {
                if (!string.IsNullOrEmpty(record.CacheURL)) {
                    record.CacheURL = record.CacheURL.ToLower();
                    record.CacheToken = record.CacheToken.Replace("}{", "}||{");
                    _cacheUrlRepository.Create(record);
                }
            }
            else {
                if (string.IsNullOrEmpty(record.CacheURL)) {
                    _cacheUrlRepository.Delete(_cacheUrlRepository.Get(r => r.Id == record.Id));
                }
                else {
                    record.CacheURL = record.CacheURL.ToLower();
                    record.CacheToken = record.CacheToken.Replace("}{", "}||{");
                    _cacheUrlRepository.Update(record);
                }
            }
            _cacheUrlRepository.Flush();
            _cacheAliasServices.RefreshCachedRouteConfig();// _cacheUrlRepository);         
            return RedirectToAction("Index");
        }
    }
}