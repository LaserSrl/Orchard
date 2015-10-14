using Orchard;
using Orchard.Caching;
using Orchard.Environment;
using Orchard.OutputCache.Models;
using Orchard.OutputCache.Services;
using System.Collections.Generic;
using System.Linq;

namespace Laser.Orchard.Accessibility
{
    public class AccessibilityFeature : IFeatureEventHandler
    {
        private readonly IOrchardServices _services;
        private readonly ISignals _signals;
        private readonly IOutputCacheStorageProvider _cacheStorageProvider;

        public AccessibilityFeature(IOrchardServices services, ISignals signals, IOutputCacheStorageProvider cacheStorageProvider)
        {
            _services = services;
            _signals = signals;
            _cacheStorageProvider = cacheStorageProvider;
        }

        public void Disabled(global::Orchard.Environment.Extensions.Models.Feature feature)
        {
            if (feature.Descriptor.Name == "Laser.Orchard.Accessibility")
            {
                // imposta la cache in modo che non tenga più conto del cookie "Accessibility"
                var cacheSettings = _services.WorkContext.CurrentSite.ContentItem.Parts.OfType<CacheSettingsPart>().First();

                if (cacheSettings != null)
                {
                    string vary = cacheSettings.VaryCookieStringParameters ?? "";
                    List<string> coockieList = vary.Split(',').ToList();
                    
                    if (coockieList.Contains(Utils.AccessibilityCookieName))
                    {
                        coockieList.Remove(Utils.AccessibilityCookieName);
                        vary = string.Join(",", coockieList);
                        cacheSettings.VaryCookieStringParameters = vary;
                        _signals.Trigger(CacheSettingsPart.CacheKey);
                    }
                }

                // svuota la cache
                _cacheStorageProvider.RemoveAll();
            }
        }

        public void Disabling(global::Orchard.Environment.Extensions.Models.Feature feature)
        {
            //throw new NotImplementedException();
        }

        public void Enabled(global::Orchard.Environment.Extensions.Models.Feature feature)
        {
            if (feature.Descriptor.Name == "Laser.Orchard.Accessibility")
            {
                // imposta la cache in modo che tenga conto del cookie "Accessibility"
                var cacheSettings = _services.WorkContext.CurrentSite.ContentItem.Parts.OfType<CacheSettingsPart>().First();

                if (cacheSettings != null)
                {
                    string vary = cacheSettings.VaryCookieStringParameters ?? "";
                    if (vary.Contains(Utils.AccessibilityCookieName) == false)
                    {
                        if (string.IsNullOrWhiteSpace(vary))
                        {
                            vary = Utils.AccessibilityCookieName;
                        }
                        else
                        {
                            vary += "," + Utils.AccessibilityCookieName;
                        }
                        cacheSettings.VaryCookieStringParameters = vary;
                        _signals.Trigger(CacheSettingsPart.CacheKey);
                    }
                }

                // svuota la cache
                _cacheStorageProvider.RemoveAll();
            }
        }

        public void Enabling(global::Orchard.Environment.Extensions.Models.Feature feature)
        {
            //throw new NotImplementedException();
        }

        public void Installed(global::Orchard.Environment.Extensions.Models.Feature feature)
        {
            //throw new NotImplementedException();
        }

        public void Installing(global::Orchard.Environment.Extensions.Models.Feature feature)
        {
            //throw new NotImplementedException();
        }

        public void Uninstalled(global::Orchard.Environment.Extensions.Models.Feature feature)
        {
            //throw new NotImplementedException();
        }

        public void Uninstalling(global::Orchard.Environment.Extensions.Models.Feature feature)
        {
            //throw new NotImplementedException();
        }
    }
}