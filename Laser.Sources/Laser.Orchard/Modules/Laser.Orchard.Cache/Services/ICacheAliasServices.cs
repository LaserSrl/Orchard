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

        void RefreshCachedRouteConfig();
    }

    public class CacheAliasServices : ICacheAliasServices {
        public static List<CacheRouteConfig> CachedRouteConfig;

        public void RefreshCachedRouteConfig() {

            /// Questa classe non utilizza la dependency injection per IRepository<CacheUrlRecord>
            /// in quanto questa classe e la classe di gestione della cache (il controller CacheUrlAdminController) non vengono
            /// richiamate nella stessa pipeline.

            var defaultMaxAge = _orchardServices.WorkContext.CurrentSite.As<CacheSettingsPart>().DefaultMaxAge;
            IRepository<CacheUrlRecord> _tmpcacheUrlRepository;
            _orchardServices.WorkContext.TryResolve<IRepository<CacheUrlRecord>>(out _tmpcacheUrlRepository);
            if (_tmpcacheUrlRepository.Fetch(x => x.CacheURL == "user+info").FirstOrDefault() == null) {
                _tmpcacheUrlRepository.Create(new CacheUrlRecord {
                    CacheDuration = 0,
                    CacheGraceTime = 0,
                    Priority = 10,
                    CacheURL = "user+info",
                    CacheToken = "{User.Id}"
                });
                _tmpcacheUrlRepository.Flush();
            }
            if (_tmpcacheUrlRepository.Fetch(x => x.CacheURL == "user info").FirstOrDefault() == null) {
                _tmpcacheUrlRepository.Create(new CacheUrlRecord {
                    CacheDuration = 0,
                    CacheGraceTime = 0,
                    Priority = 10,
                    CacheURL = "user info",
                    CacheToken = "{User.Id}"
                });
                _tmpcacheUrlRepository.Flush();
            }
            CachedRouteConfig = _tmpcacheUrlRepository.Table.OrderByDescending(y => y.Priority).Select(w => new CacheRouteConfig {
                Duration = w.CacheDuration,
                GraceTime = w.CacheGraceTime,
                Priority = w.Priority,
                FeatureName = "CacheUrl",
                MaxAge = defaultMaxAge,
                RouteKey = w.CacheURL,
                Url =  w.CacheToken
            }).ToList();
        }

        public IOrchardServices _orchardServices { get; set; }

        public CacheAliasServices(OrchardServices orchardServices) {
            _orchardServices = orchardServices;
            RefreshCachedRouteConfig();
        }

        public CacheRouteConfig GetByUrl(string url) {
            if (CachedRouteConfig != null) {
                return CachedRouteConfig.Where(x => url.ToLower().Contains(x.RouteKey)).OrderByDescending(w => w.Priority).FirstOrDefault();
            }
            return null;
        }
    }
}