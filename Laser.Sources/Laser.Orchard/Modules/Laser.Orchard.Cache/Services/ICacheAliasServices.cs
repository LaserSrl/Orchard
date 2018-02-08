using System.Collections.Generic;
using System.Linq;
using Laser.Orchard.Cache.Models;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Data;
using Orchard.Environment;

using Orchard.OutputCache.Models;

namespace Laser.Orchard.Cache.Services {

    public interface ICacheAliasServices : ISingletonDependency {

        CacheRouteConfig GetByUrl(string url);

        void RefreshCachedRouteConfig(IRepository<CacheUrlRecord> _cacheUrlRepository);
    }

    public class CacheAliasServices : ICacheAliasServices {
        public static List<CacheRouteConfig> CachedRouteConfig;
        private IRepository<CacheUrlRecord> _cacheUrlRepository;

        public void RefreshCachedRouteConfig(IRepository<CacheUrlRecord> _cacheUrlRepository) {
            var defaultMaxAge = _orchardServices.WorkContext.CurrentSite.As<CacheSettingsPart>().DefaultMaxAge;
            CachedRouteConfig = _cacheUrlRepository.Fetch(x => x.Id > 0).OrderByDescending(y => y.Priority).Select(w => new CacheRouteConfig {
                Duration = w.CacheDuration,
                GraceTime = w.CacheGraceTime,
                Priority = w.Priority,
                FeatureName = "CacheUrl",
                MaxAge = defaultMaxAge,
                RouteKey = w.CacheURL,
                Url = "CacheUrl" + w.CacheToken
            }).ToList();
        }

        public IOrchardServices _orchardServices { get; set; }

        public CacheAliasServices(OrchardServices orchardServices, IRepository<CacheUrlRecord> cacheUrlRepository) {
            _orchardServices = orchardServices;
            _cacheUrlRepository = cacheUrlRepository;
            RefreshCachedRouteConfig(_cacheUrlRepository);
        }

        public CacheRouteConfig GetByUrl(string url) {
            if (CachedRouteConfig != null) {
                return CachedRouteConfig.Where(x => url.ToLower().Contains(x.RouteKey)).OrderByDescending(w => w.Priority).FirstOrDefault();
            }
            return null;
        }
    }
}